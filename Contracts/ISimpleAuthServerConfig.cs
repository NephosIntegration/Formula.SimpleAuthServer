using System;
using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4.Test;


namespace Formula.SimpleAuthServer
{
    public interface ISimpleAuthServerConfig
    {
        IEnumerable<IdentityResource> GetIdentityResources();
        IEnumerable<ApiResource> GetApiResources();
        IEnumerable<Client> GetClients();
        List<TestUser> GetTestUsers();
    }
}