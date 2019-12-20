using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formula.Core;
using Formula.SimpleMembership;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Formula.SimpleAuthServer
{
    public class AuthAccountService : MembershipAccountService
    {
        protected readonly IAuthenticationSchemeProvider _schemeProvider;
        protected readonly IIdentityServerInteractionService _interaction;
        protected readonly IClientStore _clientStore;
        protected readonly IEventService _events;

        /// Delete These : Begin
        public IAuthenticationSchemeProvider GetSchemeProvider()
        {
            return _schemeProvider;
        }

        public IIdentityServerInteractionService GetInteraction()
        {
            return _interaction;
        }

        public IClientStore GetClientStore()
        {
            return _clientStore;
        }

        public IEventService GetEventService()
        {
            return _events;
        }
        /// Delete These : End

        public AuthAccountService(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthenticationSchemeProvider schemeProvider,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events)
            : base(userManager, signInManager)
        {
            _schemeProvider = schemeProvider;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
        }

        public Task<URLContextService> GetContextAsync(String returnUrl)
        {
            return URLContextService.GetAsync(_interaction, _clientStore, returnUrl);
        }

        public override async Task<StatusBuilder> LoginAsync(LoginDetails loginDetails)
        {
            var results = await base.LoginAsync(loginDetails);

            if (results.IsSuccessful)
            {
                var user = results.GetDataAs<ApplicationUser>();
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));
            }
            else
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(loginDetails.Username, results.Message));
            }

            return results;
        }

        public async Task<URLContextService> CancelLoginAsync(String returnUrl)
        {
            var context = await GetContextAsync(returnUrl);
            if (context.TrustType != URLTrustType.Unknown)
            {
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.GrantConsentAsync(context.AuthorizationRequest, ConsentResponse.Denied);
            }

            return context;
        }
    }
}
