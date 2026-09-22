using Microsoft.AspNetCore.Mvc;

namespace Workhub_Web.Controllers
{
    [Route("Employer/Home")]
    public class CompanyHomeController : Controller
    {
        // =========================================
        // COMPANY HOME
        //
        // GET:
        // /Employer/Home
        // =========================================

        [HttpGet("")]
        public IActionResult Index()
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }

            ViewBag.CompanyFullName =
                HttpContext.Session
                    .GetString("FullName")
                ?? "Company";

            ViewBag.CompanyEmail =
                HttpContext.Session
                    .GetString("Email")
                ?? "";

            return View();
        }


        // =========================================
        // COMPANY LOGIN CHECK
        // =========================================

        private bool IsCompanyLoggedIn()
        {
            var role =
                HttpContext.Session
                    .GetString("Role");

            var token =
                HttpContext.Session
                    .GetString("JwtToken")
                ??
                HttpContext.Session
                    .GetString("CompanyJwtToken");

            return
                !string.IsNullOrWhiteSpace(token)
                &&
                string.Equals(
                    role,
                    "Company",
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}