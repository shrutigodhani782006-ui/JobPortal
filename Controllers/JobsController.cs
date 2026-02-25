using JobPortal.Data;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------- PUBLIC JOB LIST ----------------
        [AllowAnonymous]
        public async Task<IActionResult> Index(string search, string location, string skill)
        {
            var jobs = _context.Jobs
                               .Include(j => j.Company)
                               .Where(j => j.Company != null && j.IsActive)
                               .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                jobs = jobs.Where(j => j.Title.Contains(search));
            if (!string.IsNullOrEmpty(location))
                jobs = jobs.Where(j => j.Location.Contains(location));
            if (!string.IsNullOrEmpty(skill))
                jobs = jobs.Where(j => j.Skills.Contains(skill));

            return View(await jobs.OrderByDescending(j => j.PostedAt).ToListAsync());
        }

        // ---------------- EMPLOYER JOB MANAGEMENT ----------------
        public async Task<IActionResult> MyJobs()
        {
            var uid = _userManager.GetUserId(User);
            var myJobs = await _context.Jobs
                                       .Include(j => j.Company)
                                       .Where(j => j.Company.OwnerUserId == uid)
                                       .OrderByDescending(j => j.PostedAt)
                                       .ToListAsync();
            return View(myJobs);
        }

        public IActionResult Create()
        {
            var uid = _userManager.GetUserId(User);
            var companies = _context.Companies.Where(c => c.OwnerUserId == uid).ToList();

            if (!companies.Any())
            {
                TempData["Error"] = "Please create a company before posting jobs.";
                return RedirectToAction("Index", "Companies");
            }

            ViewBag.CompanyId = new SelectList(companies, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            var uid = _userManager.GetUserId(User);
            var companies = _context.Companies.Where(c => c.OwnerUserId == uid).ToList();
            ViewBag.CompanyId = new SelectList(companies, "Id", "Name");

            if (!ModelState.IsValid) return View(job);

            job.PostedAt = DateTime.UtcNow;
            job.IsActive = true;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyJobs");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var uid = _userManager.GetUserId(User);
            var job = await _context.Jobs.Include(j => j.Company)
                                         .FirstOrDefaultAsync(j => j.Id == id && j.Company.OwnerUserId == uid);
            if (job == null) return NotFound();

            var companies = _context.Companies.Where(c => c.OwnerUserId == uid).ToList();
            ViewBag.CompanyId = new SelectList(companies, "Id", "Name", job.CompanyId);
            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job job)
        {
            if (id != job.Id) return NotFound();

            var uid = _userManager.GetUserId(User);
            var existingJob = await _context.Jobs.Include(j => j.Company)
                                                 .FirstOrDefaultAsync(j => j.Id == id && j.Company.OwnerUserId == uid);
            if (existingJob == null) return NotFound();

            var companies = _context.Companies.Where(c => c.OwnerUserId == uid).ToList();
            ViewBag.CompanyId = new SelectList(companies, "Id", "Name", job.CompanyId);

            if (!ModelState.IsValid) return View(job);

            existingJob.Title = job.Title;
            existingJob.Description = job.Description;
            existingJob.Location = job.Location;
            existingJob.Skills = job.Skills;
            existingJob.SalaryFrom = job.SalaryFrom;
            existingJob.SalaryTo = job.SalaryTo;
            existingJob.EmploymentType = job.EmploymentType;
            existingJob.CompanyId = job.CompanyId;
            existingJob.IsActive = job.IsActive;

            _context.Update(existingJob);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyJobs");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var uid = _userManager.GetUserId(User);
            var job = await _context.Jobs.Include(j => j.Company)
                                         .FirstOrDefaultAsync(j => j.Id == id && j.Company.OwnerUserId == uid);
            if (job == null) return NotFound();
            return View(job);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uid = _userManager.GetUserId(User);
            var job = await _context.Jobs.Include(j => j.Company)
                                         .FirstOrDefaultAsync(j => j.Id == id && j.Company.OwnerUserId == uid);

            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyJobs");
        }

        // ---------------- JOB DETAILS ----------------
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs.Include(j => j.Company)
                                         .FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return NotFound();

            if (User.Identity.IsAuthenticated && User.IsInRole("JobSeeker"))
            {
                var userId = _userManager.GetUserId(User);
                var application = await _context.JobApplications
                                                .FirstOrDefaultAsync(a => a.JobId == id && a.ApplicantId == userId);
                ViewBag.HasApplied = application != null;
                ViewBag.ApplicationId = application?.Id;
            }

            return View(job);
        }
    }
}
