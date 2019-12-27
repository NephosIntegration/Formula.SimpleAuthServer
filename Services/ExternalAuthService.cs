using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Formula.SimpleMembership;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using IdentityModel;
using Formula.Core;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Events;

namespace Formula.SimpleAuthServer
{
    public class ExternalAuthService : MembershipAccountService
    {
        protected readonly IIdentityServerInteractionService _interaction;
        protected readonly IClientStore _clientStore;
        protected readonly IEventService _events;

        public ExternalAuthService(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events)
            : base(userManager, signInManager)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
        }

        public StatusBuilder ValidateReturnUrl(HttpContext context, IUrlHelper urlHelper, String returnUrl)
        {
            var output = new StatusBuilder();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            var redirectDetails = new RedirectUrlDetails();
            redirectDetails.IsLocal = false;

            if (_interaction.IsValidReturnUrl(redirectDetails.Url) || urlHelper.IsLocalUrl(redirectDetails.Url))
            {
                redirectDetails.IsLocal = true;
            }
            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            else if (urlHelper.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                output.RecordFailure("Invalid return URL - Possible malicious Link.");
            }

            return output;
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        public async Task<StatusBuilder> HandleExternalAuthAsync(HttpContext context, IUrlHelper urlHelper)
        {
            var output = new StatusBuilder();

            // Attempt to authenticate with IdetityConstants.ExternalScheme first
            var schemeUsed = IdentityConstants.ExternalScheme;
            var result = await context.AuthenticateAsync(schemeUsed);
            if (result?.Succeeded != true)
            {
                // If that fails try with idsrv.external
                schemeUsed = "idsrv.external";
                result = await context.AuthenticateAsync(schemeUsed);
                if (result?.Succeeded != true)
                {
                    output.RecordFailure("External authentication error");
                }
            }

            // If we were successfully authenticated
            if (output.IsSuccessful)
            {
                // lookup our user and external provider info
                var (user, provider, providerUserId, claims) = await this.FindUserFromExternalProviderAsync(result);
                if (user == null)
                {
                    // this might be where you might initiate a custom workflow for user registration
                    // in this sample we don't show how that would be done, as our sample implementation
                    // simply auto-provisions new external user
                    user = await this.AutoProvisionUserAsync(provider, providerUserId, claims);
                }

                // this allows us to collect any additonal claims or properties
                // for the specific prtotocols used and store them in the local auth cookie.
                // this is typically used to store data needed for signout from those protocols.
                var additionalLocalClaims = new List<Claim>();
                var localSignInProps = new AuthenticationProperties();
                this.ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
                this.ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
                this.ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

                // issue authentication cookie for user
                // we must issue the cookie maually, and can't use the SignInManager because
                // it doesn't expose an API to issue additional claims from the login workflow
                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                additionalLocalClaims.AddRange(principal.Claims);
                var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;
                await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name));
                await context.SignInAsync(user.Id, name, provider, localSignInProps, additionalLocalClaims.ToArray());

                // delete temporary cookie used during external authentication
                await context.SignOutAsync(schemeUsed);

                output = ValidateReturnUrl(context, urlHelper, result.Properties.Items["returnUrl"]);
            }

            return output;
        }

        // Returns true or fale if windows login is successful, on true, it returns the redirect URL
        public async Task<StatusBuilder> ProcessWindowsLoginAsync(HttpContext context, IUrlHelper urlHelper, string returnUrl)
        {
            var output = new StatusBuilder();

            // see if windows auth has already been requested and succeeded
            var result = await context.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, treating windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = urlHelper.Action("Callback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await context.SignInAsync(
                    IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                
                output.SetData(props.RedirectUri);
            }
            else
            {
                output.Fail();
            }

            return output;
        }


        public async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
            FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        public async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            // user's display name
            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            if (name != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, name));
            }
            else
            {
                var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            // email
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
               claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }

            var user = new ApplicationUser
            {
                UserName = Guid.NewGuid().ToString(),
            };
            var identityResult = await _userManager.CreateAsync(user);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            if (filtered.Any())
            {
                identityResult = await _userManager.AddClaimsAsync(user, filtered);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
            }

            identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            return user;
        }        

        public void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }

        public void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        public void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }


    }
}