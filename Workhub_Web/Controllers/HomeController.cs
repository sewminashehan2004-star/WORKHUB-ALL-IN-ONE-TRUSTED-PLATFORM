using Microsoft.AspNetCore.Mvc;
using Workhub_Web.Models;
using System.Diagnostics;

namespace Workhub_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // =====================================
        // HOME
        // =====================================

        public IActionResult Index()
        {
            return View();
        }

        // =====================================
        // ABOUT
        // =====================================

        public IActionResult About()
        {
            return View();
        }

        // =====================================
        // CONTACT
        // =====================================

        public IActionResult Contact()
        {
            return View();
        }

        // =====================================
        // PRIVACY
        // =====================================

        public IActionResult Privacy()
        {
            return View();
        }

        // =====================================
        // ERROR
        // =====================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}