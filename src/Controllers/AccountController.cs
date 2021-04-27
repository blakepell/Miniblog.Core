using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Miniblog.Core.Services;

namespace Miniblog.Core.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserServices _userServices;

        public AccountController(IUserServices userServices)
        {
            _userServices = userServices;
        }


        [Route("/login")]
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        [Route("/login")]
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(string returnUrl, LoginViewModel model)
        {
            this.ViewData["ReturnUrl"] = returnUrl;

            if (this.ModelState.IsValid && _userServices.ValidateUser(model.UserName, model.Password))
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));

                var principle = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties {IsPersistent = model.RememberMe};
                await this.HttpContext.SignInAsync(principle, properties);

                return this.LocalRedirect(returnUrl ?? "/");
            }

            this.ModelState.AddModelError(string.Empty, "Username or password is invalid.");
            return this.View("Login", model);
        }

        [Route("/logout")]
        public async Task<IActionResult> LogOutAsync()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return this.LocalRedirect("/");
        }
    }
}
