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
using System.Text.Encodings.Web;

namespace Formula.SimpleAuthServer
{
    public class TwoFactorAuthService : TwoFactorService
    {

        public TwoFactorAuthService(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder,
            IEmailSender emailSender,
            ISmsSender smsSender)
            : base(userManager, signInManager, urlEncoder, emailSender, smsSender)
        {
        }

   }
}