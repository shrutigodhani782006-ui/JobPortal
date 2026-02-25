using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace JobPortal.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserManager<ApplicationUser> userManager,
                             IUserStore<ApplicationUser> userStore,
                             SignInManager<ApplicationUser> signInManager,
                             RoleManager<IdentityRole> roleManager,
                             ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string FullName { get; set; }
            public string College { get; set; }
            public bool IsEmployer { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Redirect already logged-in users to dashboard
            if (User.Identity!.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                    return RedirectToAction("Dashboard", "Home");
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Home/Dashboard");

            if (!ModelState.IsValid)
                return Page();

            var user = new ApplicationUser
            {
                FullName = Input.FullName,
                College = Input.College,
                IsEmployer = Input.IsEmployer
            };

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account.");

                // Ensure roles exist
                if (!await _roleManager.RoleExistsAsync("JobSeeker"))
                    await _roleManager.CreateAsync(new IdentityRole("JobSeeker"));
                if (!await _roleManager.RoleExistsAsync("Employer"))
                    await _roleManager.CreateAsync(new IdentityRole("Employer"));

                // Assign role
                if (Input.IsEmployer)
                    await _userManager.AddToRoleAsync(user, "Employer");
                else
                    await _userManager.AddToRoleAsync(user, "JobSeeker");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The user store must support email.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
