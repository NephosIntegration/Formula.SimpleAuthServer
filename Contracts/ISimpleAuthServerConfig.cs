using System;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Formula.SimpleAuthServer
{
    public interface ISimpleAuthServerConfig
    {
        IEnumerable<ApiResource> GetApiResources();
        IEnumerable<Client> GetClients();
    }
}