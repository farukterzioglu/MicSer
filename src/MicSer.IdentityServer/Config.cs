// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace MicSer.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("spend"),
                new ApiScope("list")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client() 
                {
                    ClientName = "Spender",
                    ClientId = "spender-app",
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) }, // 7d4edf45bfa0e987659e2b92aaf88e2d636475ad4cfccb8fb47eb3348f629c5f
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AccessTokenLifetime = 180,
                    AllowedScopes = { "spend", "list" },
                    AllowAccessTokensViaBrowser = true
                }
            };

        public static List<TestUser> TestUsers => new List<TestUser>(){
            new TestUser() {
                Username = "faruk",
                Password = "123",
                SubjectId = "sub1"
            }
        };
    }
}