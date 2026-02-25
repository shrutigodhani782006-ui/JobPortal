using JobPortal.Data;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Employer")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompaniesController(ApplicationDbContext context,
                                   UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Show only the logged-in employer’s companies
        public async Task<IActionResult> Index()
        {
            var uid = _userManager.GetUserId(User);
            var list = await _context.Companies
                                     .Where(c => c.OwnerUserId == uid)
                                     .ToListAsync();
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (!ModelState.IsValid) return View(company);

            company.OwnerUserId = _userManager.GetUserId(User);
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var uid = _userManager.GetUserId(User);
            var c = await _context.Companies
                                  .FirstOrDefaultAsync(x => x.Id == id && x.OwnerUserId == uid);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company)
        {
            if (id != company.Id) return NotFound();
            if (!ModelState.IsValid) return View(company);

            // preserve the OwnerUserId
            var existing = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();
            company.OwnerUserId = existing.OwnerUserId;

            _context.Update(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var uid = _userManager.GetUserId(User);
            var c = await _context.Companies.FirstOrDefaultAsync(x => x.Id == id && x.OwnerUserId == uid);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uid = _userManager.GetUserId(User);
            var c = await _context.Companies.FirstOrDefaultAsync(x => x.Id == id && x.OwnerUserId == uid);
            if (c != null)
            {
                _context.Companies.Remove(c);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
