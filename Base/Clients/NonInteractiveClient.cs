using System;
using IdentityServer4.Models;

namespace Formula.SimpleAuthServer
{
    public class NonInteractiveClient : SimpleClientBuilderBase
    {
        public NonInteractiveClient(SimpleClientDetails clientDetails) : base(clientDetails)
        {
            // no interactive user, use the clientid/secret for authentication
            AllowedGrantTypes = GrantTypes.ClientCredentials;
        }
    }
}