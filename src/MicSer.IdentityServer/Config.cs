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
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope("secured-api", "Secured API")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client() 
                {
                    ClientId = "spender",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                    AllowedScopes = { "spend", "list" }
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