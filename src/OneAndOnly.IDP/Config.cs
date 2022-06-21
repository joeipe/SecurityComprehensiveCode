﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace OneAndOnly.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("roles", "Your role(s)", new List<string>() { "role" }),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("simpleaapi.fullaccess", "Full access to Simple A API"),
                new ApiScope("simplebapi.fullaccess", "Full access to Simple B API")
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource()
                {
                    Name = "simpleaapi",
                    DisplayName = "Simple A API",
                    Scopes = { "simpleaapi.fullaccess" },
                    UserClaims = new List<string>() { "role", "given_name" }
                },
                new ApiResource()
                {
                    Name = "simplebapi",
                    DisplayName = "Simple B API",
                    Scopes = { "simplebapi.fullaccess" },
                    UserClaims = new List<string>() { "role" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new Client[] 
            { 
                new Client
                {
                    ClientName = "Simple SPA",
                    ClientId = "simplespaclient",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = new List<string>() { "https://localhost:44302/signin-oidc" },
                    PostLogoutRedirectUris = new List<string>() { "https://localhost:44302/signout-callback-oidc" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "simpleaapi.fullaccess",
                        "simplebapi.fullaccess"
                    },

                    //refresh token related
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                }
            };
    }
}