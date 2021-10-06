using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandroidWorxApp.BusinessLogic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace LandroidWorxApp.Pages
{
    public class DeleteUserDataModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IManager _manager;

        public DeleteUserDataModel(IManager manager, IConfiguration configuration)
        {
            _manager = manager;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var username = HttpContext.User.Identity.Name;
            _manager.DeleteUserData(username);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/login");
        }
    }
}
