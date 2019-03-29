// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Quickstart.UI
{
    public class TestUsers
    {
        public static List<TestUser> Users = new List<TestUser>
        {

            new TestUser{SubjectId = "818727", Username = "admin", Password = "@dminPassword!",
                Claims =
                {
                    new Claim(JwtClaimTypes.Role, "admin"),
                    new Claim(JwtClaimTypes.Name, "Admin"),
                    new Claim(JwtClaimTypes.GivenName, "Administrator"),
                    //new Claim(JwtClaimTypes.FamilyName, "Admin"),
                    new Claim(JwtClaimTypes.Email, "admin@yopmail.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "https://github.com/Moleculars/IdentityServer4"),
                    //new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
        };
    }
}