using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using MvcClient2.Models.AccountViewModels;
using MvcClient2.Services;

namespace MvcClient2.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly string _externalCookieScheme;

        public AccountController(
            IOptions<IdentityCookieOptions> identityCookieOptions,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var disco = await DiscoveryClient.GetAsync("http://localhost:5000");

                var tokenClient = new TokenClient(disco.TokenEndpoint, "mvc3", "secret");
                var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(model.Email, model.Password, "api1");
                if (!tokenResponse.IsError)
                {
                  
                    var keys = new List<SecurityKey>();
                    foreach (var webKey in disco.KeySet.Keys)
                    {
                        var e = Base64Url.Decode(webKey.E);
                        var n = Base64Url.Decode(webKey.N);

                        var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                        {
                            KeyId = webKey.Kid
                        };

                        keys.Add(key);
                    }
                    var parameters = new TokenValidationParameters
                    {
                        ValidIssuer = disco.Issuer,
                        ValidAudience = "api1",
                        IssuerSigningKeys = keys,
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                        SaveSigninToken = true
                    };
                    var handler = new JwtSecurityTokenHandler();
                    handler.InboundClaimTypeMap.Clear();
                    var user = handler.ValidateToken(tokenResponse.AccessToken, parameters, out var _);
                     await HttpContext.Authentication.SignInAsync("Cookies", user);
              
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, tokenResponse.Error);
                    return View(model);
                }
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            return Redirect("/Home/Index");
        }
        
        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
