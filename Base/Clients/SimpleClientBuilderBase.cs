using System;
using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4.Test;
using IdentityServer4;

namespace Formula.SimpleAuthServer
{
    public abstract class SimpleClientBuilderBase : Client
    {
        private SimpleClientDetails _clientDetails;

        public SimpleClientBuilderBase(SimpleClientDetails clientDetails)
        {
            this._clientDetails = clientDetails;

            this.ClientId = clientDetails.ClientId;
            this.ClientName = clientDetails.ClientName ?? clientDetails.ClientId;

            if (clientDetails.RedirectUris != null && clientDetails.RedirectUris.Count > 0)
            {
                this.RedirectUris = new List<String>();
                foreach(var redirectUri in clientDetails.RedirectUris)
                {
                    this.RedirectUris.Add(this.GetFullUri(clientDetails.BaseUri, redirectUri));
                }                
            }

            this.PostLogoutRedirectUris = new List<String>
            {
                this.GetFullUri(clientDetails.BaseUri, clientDetails.LogoutRedirectUri)
            };

            if (String.IsNullOrEmpty(clientDetails.Secret) == false)
            {
                this.ClientSecrets = new List<Secret>();
                this.ClientSecrets.Add( new Secret(clientDetails.Secret.Sha256()));
            }

            this.AllowedScopes = new List<String>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
            };

            if (clientDetails.AllowedScopes != null && clientDetails.AllowedScopes.Count > 0)
            {
                foreach(var scope in clientDetails.AllowedScopes)
                {
                    this.AllowedScopes.Add(scope);
                }                
            }
        }

        protected virtual String GetFullUri(string baseUri, string path)
        {
            return baseUri + "/" + path;
        }
    }
}