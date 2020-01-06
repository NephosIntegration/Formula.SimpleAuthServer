using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleAuthServer
{
    public class SimpleClientDetails
    {
        //
        // Summary:
        //     Unique ID of the client
        public string ClientId { get; set; }
        //
        // Summary:
        //     Client display name (used for logging and consent screen)
        public string ClientName { get; set; }
        //
        // Summary:
        //     Client secret - only relevant for flows that require a secret
        public string Secret { get; set; }
        //
        // Summary:
        //     Specifies the base URI at client
        public string BaseUri { get; set; }
        //
        // Summary:
        //     Specifies URI to return tokens or authorization codes to
        public ICollection<string> RedirectUris { get; set; }
        //
        // Summary:
        //     Specifies URI to redirect to after logout
        public string LogoutRedirectUri { get; set; }
        //
        // Summary:
        //     Specifies the api scopes that the client is allowed to request. If empty, the
        //     client can't access any scope
        public ICollection<string> AllowedScopes { get; set; }
    }
}