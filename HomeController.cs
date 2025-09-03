using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BajajCentra.Models;
using Microsoft.AspNetCore.Authorization; // <-- Add this

namespace BajajCentra.Controllers
{
    [Authorize] // <-- Add this attribute
    public class HomeController : Controller
    {
        // ... rest of the code is unchanged ...
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
