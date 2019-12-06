using System;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Formula.SimpleAuthServer
{
    public class SimpleAuthServerConfigDemo : SimpleAuthServerConfigBase
    {
        public static SimpleAuthServerConfigDemo Get() 
        {
            return new SimpleAuthServerConfigDemo();
        }
    }
    
    public abstract class SimpleAuthServerConfigBase : ISimpleAuthServerConfig
    {
        public virtual IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "My API")
            };
        }

        public virtual IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "api" }
                }
            };

        }
    }
}