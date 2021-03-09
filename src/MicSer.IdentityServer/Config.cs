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
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AccessTokenLifetime = 180,
                    AllowedScopes = { "spend", "list" },
                    AllowAccessTokensViaBrowser = true
                },
                new Client() 
                {
                    ClientName = "Api Consumer",
                    ClientId = "api-consumer",
                    ClientSecrets = { new Secret("e1844b86-6eb6-4ed8-95f3-aa3656672ede".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AccessTokenLifetime = 60,
                    AllowedScopes = { "list" }
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