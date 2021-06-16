using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using LandroidWorxApp.BusinessLogic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace LandroidWorxApp.Pages
{

    public class LoginActionModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILsClientWeb _lsClientWeb;

        public LoginActionModel(ILsClientWeb lsClientWeb, IConfiguration configuration)
        {
            _lsClientWeb = lsClientWeb;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnPostAsync(string username, string password, bool rememberMe, string returnUrl)
        {
            returnUrl = returnUrl ?? Url.Content("~/products");
            try
            {
                // Clear the existing external cookie
                await HttpContext
                    .SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }

            LsClientWeb_LoginResponse response = new LsClientWeb_LoginResponse();
           
            try
            {
                response = await _lsClientWeb.Login(new LsClientWeb_LoginRequest()
                {
                    ClientSecret = _configuration.GetValue<string>("ClientSecret"),
                    GrantType = "password",
                    Scope = "*",
                    Username = username,
                    Password = password
                });
            }
            catch (WebException ex)
            {
                if((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                    return LocalRedirect(Url.Content("~/login/invalidAuth"));
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim("BearerToken", response.BearerToken),
                new Claim("BrokerUrl", response.BrokerUrl),
            };
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                RedirectUri = this.Request.Host.Value
            };
            try
            {
           
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return LocalRedirect(returnUrl);
        }
    }
}

