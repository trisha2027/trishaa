using Microsoft.AspNetCore.Mvc;
using BajajCentra.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq; // Required for .SelectMany()

namespace BajajCentra.Controllers
{
    public class AccountController : Controller
    {
        // This method shows the login page.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // This method handles the form submission from your JavaScript.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // First, check if the data sent from the form is valid.
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            // If valid, check the credentials.
            // NOTE: Replace this with a real database check in a production app.
            if (model.Username == "bajaj" && model.Password == "centra@2025")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim("FullName", "Bajaj User"),
                    new Claim(ClaimTypes.Role, "User"),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Return a success message with the redirect URL.
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            else
            {
                // If credentials are wrong, return an error message.
                return Json(new { success = false, message = "Invalid username or password." });
            }
        } // <-- The error was caused by this closing brace being missing.

        // This method handles logging out.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
