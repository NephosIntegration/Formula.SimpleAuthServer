using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;

namespace Formula.SimpleAuthServer
{
    public enum URLTrustType
    {
        Unknown = 0,
        Known = 1,
        Native = 2,
    }

    public class URLContextService
    {
        protected readonly IIdentityServerInteractionService _interaction;
        protected readonly IClientStore _clientStore;


        public URLContextService(IIdentityServerInteractionService interaction, IClientStore clientStore)
        {
            _interaction = interaction;
            _clientStore = clientStore;
        }

        protected String _url;
        public String Url
        {
            get { return this._url; }
        }

        protected URLTrustType _urlTrustType = URLTrustType.Unknown;
        public URLTrustType TrustType
        {
            get { return this._urlTrustType; }
        }

        protected AuthorizationRequest _authorizationRequest;
        public AuthorizationRequest AuthorizationRequest
        {
            get { return this._authorizationRequest; }
        }

        /// <summary>
        /// Determines whether the client is configured to use PKCE.
        /// </summary>
        /// <param name="client_id">The client identifier.</param>
        /// <returns></returns>
        public async Task<bool> IsPkceClientAsync(string client_id)
        {
            if (!string.IsNullOrWhiteSpace(client_id))
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(client_id);
                return client?.RequirePkce == true;
            }

            return false;
        }

        public async Task<URLContextService> SetUrl(String url)
        {

            this._url = url;
            this._urlTrustType = URLTrustType.Unknown; // Assume we don't know / trust the return URL

            // check if we are in the AuthorizationRequest of an authorization request
            this._authorizationRequest = await _interaction.GetAuthorizationContextAsync(this.Url);

            if (this.AuthorizationRequest != null)
            {
                this._urlTrustType = URLTrustType.Known;  // It is at least a known / trusted URL

                // we can trust model.ReturnUrl since GetAuthorizationAuthorizationRequestAsync returned non-null
                if (await IsPkceClientAsync(this.AuthorizationRequest.ClientId))
                {
                    // if the client is PKCE then we assume it's native
                    this._urlTrustType = URLTrustType.Native;
                }
            }

            return this;
        }

        public static async Task<URLContextService> GetAsync(IIdentityServerInteractionService interaction, IClientStore clientStore, String url)
        {
            var instance = new URLContextService(interaction, clientStore);

            return await instance.SetUrl(url);
        }
    }
}
