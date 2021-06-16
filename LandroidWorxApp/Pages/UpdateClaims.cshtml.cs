using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LandroidWorxApp.Pages
{
    public class UpdateClaimsModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync(string serialNo, string productName, string cmdInPath, string cmdOutPath, string returnUrl)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            identity.AddClaim(new Claim("SelectedProductSN", serialNo));
            identity.AddClaim(new Claim("SelectedProductName", productName));
            identity.AddClaim(new Claim("CmdInPath", cmdInPath));
            identity.AddClaim(new Claim("CmdOutPath", cmdOutPath));

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = this.Request.Host.Value
            };

            await HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
               new ClaimsPrincipal(identity),
               authProperties);

            returnUrl = returnUrl ?? Url.Content("~/");
            return LocalRedirect(returnUrl);    
        }
    }
}
