// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Host.Configuration;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Quickstart.UI;
using idunno.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Host
{
    public partial class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            var connectionString = this._config.GetConnectionString("db");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddDbContext<ApplicationDbContext>(builder =>
                builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;

                    options.MutualTls.Enabled = true;
                    options.MutualTls.ClientCertificateAuthenticationScheme = "x509";
                })

                .AddOperationalStore(options =>
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddConfigurationStore(options =>
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddAspNetIdentity<IdentityUser>()
                //.AddInMemoryClients(Clients.Get())
                //.AddInMemoryIdentityResources(Resources.GetIdentityResources())
                //.AddInMemoryApiResources(Resources.GetApiResources())
                //.AddTestUsers(Users.Get())

                //.AddInMemoryClients(Clients.Get())
                //.AddInMemoryClients(_config.GetSection("Clients"))
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddDeveloperSigningCredential()
                .AddExtensionGrantValidator<Extensions.ExtensionGrantValidator>()
                .AddExtensionGrantValidator<Extensions.NoSubjectExtensionGrantValidator>()
                .AddJwtBearerClientAuthentication()
                .AddAppAuthRedirectUriValidator()
                .AddTestUsers(TestUsers.Users)
                .AddMutualTlsSecretValidators();

            services.AddExternalIdentityProviders();
            services.AddLocalApiAuthentication();

            services.AddAuthentication()
               .AddCertificate("x509", options =>
               {
                   options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                   options.Events = new CertificateAuthenticationEvents
                   {
                       OnValidateCertificate = context =>
                       {
                           context.Principal = Principal.CreateFromCertificate(context.ClientCertificate, includeAllClaims: true);
                           context.Success();

                           return Task.CompletedTask;
                       }
                   };
               });


            return services.BuildServiceProvider(validateScopes: true);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<Logging.RequestLoggerMiddleware>();
            app.UseDeveloperExceptionPage();

            // InitializeDbTestData(app);

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }


        //private static void InitializeDbTestData(IApplicationBuilder app)
        //{
        //    using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        //    {
        //        scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
        //        scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
        //        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

        //        var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

        //        if (!context.Clients.Any())
        //        {
        //            foreach (IdentityServer4.Models.Client client in Clients.Get())
        //                context.Clients.Add(client.ToEntity());
        //            context.SaveChanges();
        //        }

        //        if (!context.IdentityResources.Any())
        //        {
        //            foreach (var resource in Resources.GetIdentityResources())
        //                context.IdentityResources.Add(resource.ToEntity());
        //            context.SaveChanges();
        //        }

        //        if (!context.ApiResources.Any())
        //        {
        //            foreach (var resource in Resources.GetApiResources())
        //                context.ApiResources.Add(resource.ToEntity());
        //            context.SaveChanges();
        //        }

        //        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        //        if (!userManager.Users.Any())
        //            foreach (var testUser in TestUsers.Users)
        //            {
        //                var identityUser = new IdentityUser(testUser.Username)
        //                {
        //                    Id = Guid.NewGuid().ToString(), // testUser.SubjectId,
        //                    SecurityStamp = Guid.NewGuid().ToString(),
        //                    LockoutEnabled = false,
        //                };

        //                userManager.CreateAsync(identityUser)
        //                    .Wait();

        //                userManager.AddClaimsAsync(identityUser, testUser.Claims.ToList())
        //                    .Wait();

        //                userManager.AddPasswordAsync(identityUser, testUser.Password)
        //                    .Wait();

        //               userManager.UpdateAsync(identityUser);

        //            }


        //        // SqlException : The MERGE statement conflicted with the 
        //        // FOREIGN KEY constraint "FK_AspNetUserClaims_AspNetUsers_UserId". 
        //        // The conflict occurred in database "IdSrv4", table "dbo.AspNetUsers", column 'Id'.


        //    }
        //}

    }

}
