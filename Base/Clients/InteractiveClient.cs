using System;
using IdentityServer4.Models;

namespace Formula.SimpleAuthServer
{
    public class InteractiveClient : SimpleClientBuilderBase
    {
        public InteractiveClient(SimpleClientDetails clientDetails) : base(clientDetails)
        {
            this.AllowedGrantTypes = GrantTypes.Code;
            this.RequireConsent = false;
            this.RequirePkce = true;
            this.AllowOfflineAccess = true;
        }
    }
}