using System;
using IdentityServer4.Models;

namespace Formula.SimpleAuthServer
{
    public class BrowserClient : SimpleClientBuilderBase
    {
        public BrowserClient(SimpleClientDetails clientDetails) : base(clientDetails)
        {
            this.AllowedGrantTypes = GrantTypes.Implicit;
            this.AllowAccessTokensViaBrowser = true;
            this.AllowOfflineAccess = true;
        }
    }
}