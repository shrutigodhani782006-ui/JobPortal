using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JobPortal.Models;

namespace JobPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    if (currentUser.IsEmployer)
                    {
                        return RedirectToAction("EmployerDashboard");
                    }
                    else
                    {
                        return RedirectToAction("JobSeekerDashboard");
                    }
                }
            }

            return View(); // Public Home
        }

        public IActionResult EmployerDashboard()
        {
            return View();
        }

        public IActionResult JobSeekerDashboard()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Index");

            if (currentUser.IsEmployer)
                return View("EmployerDashboard");
            else
                return View("JobSeekerDashboard");
        }
    }
}
