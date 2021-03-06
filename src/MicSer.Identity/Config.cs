// Source: https://raw.githubusercontent.com/IdentityServer/IdentityServer4/main/samples/Quickstarts/3_AspNetCoreAndApis/src/IdentityServer/Config.cs

// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace MicSer.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId()
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[] { };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new Client[] { };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>();
        }
        
    }
}