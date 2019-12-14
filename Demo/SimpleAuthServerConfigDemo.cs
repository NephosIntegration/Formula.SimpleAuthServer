using System;
using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4.Test;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4;

namespace Formula.SimpleAuthServer
{
    public class SimpleAuthServerConfigDemo : ISimpleAuthServerConfig
    {
        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "My API")
            };
        }

        public IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                new NonInteractiveClient(new SimpleClientDetails() {
                    ClientId = "client",
                    Secret = "secret",
                    AllowedScopes = new List<String>() { "api" }
                }),
                new InteractiveClient(new SimpleClientDetails() {
                    ClientId = "OpenIDConnectDemo",
                    Secret = "secret",
                    BaseUri = "http://localhost:5002",
                    RedirectUri = "signin-oidc",
                    LogoutRedirectUri = "signout-callback-oidc",
                    AllowedScopes = new List<String>() { "api" }
                }),
                new BrowserClient(new SimpleClientDetails() {
                    ClientId = "js",
                    ClientName = "Browser Client",
                    BaseUri = "http://localhost:8080",
                    RedirectUri = "callback.html",
                    LogoutRedirectUri = "logout.html",
                    AllowedScopes = new List<String>() { "api" }
                })
            };
        }

        public List<TestUser> GetTestUsers()
        {
            return new List<TestUser> {
                new TestUser{SubjectId = "818727", Username = "alice", Password = "alice", 
                    Claims = 
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                new TestUser{SubjectId = "88421113", Username = "bob", Password = "bob", 
                    Claims = 
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("location", "somewhere")
                    }
                }    
            };
        }

        public static SimpleAuthServerConfigDemo Get() 
        {
            return new SimpleAuthServerConfigDemo();
        }
    }
}