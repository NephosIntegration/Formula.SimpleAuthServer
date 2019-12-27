using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formula.Core;
using Formula.SimpleMembership;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace Formula.SimpleAuthServer
{
    public class AccountAuthService : MembershipAccountService
    {
        protected readonly IAuthenticationSchemeProvider _schemeProvider;
        protected readonly IIdentityServerInteractionService _interaction;
        protected readonly IClientStore _clientStore;
        protected readonly IEventService _events;

        public AccountAuthService(
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





        /*******************\
        * Context Helpers
        \*******************/

        public Task<URLContextService> GetContextAsync(String returnUrl)
        {
            return URLContextService.GetAsync(_interaction, _clientStore, returnUrl);
        }

        public Task<AuthorizationRequest> GetAuthorizationContextAsync(String returnUrl)
        {
            return _interaction.GetAuthorizationContextAsync(returnUrl);
        }





        /*******************\
        * Actions
        \*******************/

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

        public Task SignOutAsync(ClaimsPrincipal user)
        {
            var output = base.SignOutAsync();
            _events.RaiseAsync(new UserLogoutSuccessEvent(user.GetSubjectId(), user.GetDisplayName()));
            return output;
        }

        public async Task<ExternalLoginDetails> GetLoginDetailsAsync(string returnUrl)
        {
            // Simple default login view model
            var loginDetails = new ExternalLoginDetails
            {
                EnableLocalLogin = false,
                ReturnUrl = returnUrl
            };

            var context = await this.GetAuthorizationContextAsync(returnUrl);

            // If there is an external identity provider
            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP
                loginDetails = new ExternalLoginDetails
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
                };
            }
            else
            {
                var schemes = await _schemeProvider.GetAllSchemesAsync();

                var providers = schemes
                    .Where(x => x.DisplayName != null ||
                                (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                    )
                    .Select(x => new ExternalProvider
                    {
                        DisplayName = x.DisplayName,
                        AuthenticationScheme = x.Name
                    }).ToList();

                var allowLocal = true;
                if (context?.ClientId != null)
                {
                    var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                    if (client != null)
                    {
                        allowLocal = client.EnableLocalLogin;

                        if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                        {
                            providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                        }
                    }
                }

                loginDetails = new ExternalLoginDetails
                {
                    AllowRememberLogin = AccountOptions.AllowRememberLogin,
                    EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = providers.ToArray()
                };
            }

            return loginDetails;
        }

        public async Task<LogoutDetails> GetLogoutDetailsAsync(ClaimsPrincipal user, string logoutId)
        {
            var logoutDetails = new LogoutDetails { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (user?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                logoutDetails.ShowLogoutPrompt = false;
                return logoutDetails;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                logoutDetails.ShowLogoutPrompt = false;
                return logoutDetails;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return logoutDetails;
        }

        public async Task<PostLogOutDetails> GetPostLogoutDetailsAsync(HttpContext context, ClaimsPrincipal user, string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var details = new PostLogOutDetails
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (user?.Identity.IsAuthenticated == true)
            {
                var idp = user.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await context.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (details.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            details.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        details.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return details;
        }
    }
}
