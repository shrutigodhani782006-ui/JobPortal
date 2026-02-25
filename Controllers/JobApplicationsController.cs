using JobPortal.Data;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace JobPortal.Controllers
{
    public class JobApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public JobApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = _userManager.GetUserId(User);
            var apps = await _context.JobApplications
                                     .Include(a => a.Job)
                                     .ThenInclude(j => j.Company)
                                     .Where(a => a.ApplicantId == userId)
                                     .OrderByDescending(a => a.AppliedAt)
                                     .ToListAsync();
            return View(apps);
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in as a job seeker to apply.";
                return RedirectToAction("Login", "Account");
            }

            var existing = await _context.JobApplications
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(a => a.JobId == jobId && a.ApplicantId == userId);
            if (existing != null)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) return NotFound();

            ViewBag.JobTitle = job.Title;
            return View(new JobApplicationViewModel { JobId = jobId });
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(JobApplicationViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
                return View(model);

            var existing = await _context.JobApplications
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(a => a.JobId == model.JobId && a.ApplicantId == userId);
            if (existing != null)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction("Details", "Jobs", new { id = model.JobId });
            }

            string resumePath = null;
            if (model.ResumeFile != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "resumes");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Guid.NewGuid() + Path.GetExtension(model.ResumeFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fileStream);
                }
                resumePath = "/resumes/" + fileName;
            }

            var application = new JobApplication
            {
                JobId = model.JobId,
                ApplicantId = userId,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Applied,
                ResumePath = resumePath,
                CoverLetter = model.CoverLetter,
                Experience = model.Experience
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been submitted!";
            return RedirectToAction("MyApplications");
        }
        [Authorize(Roles = "Employer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int appId, string status)
        {
            var application = await _context.JobApplications
                                            .Include(a => a.Job)
                                            .ThenInclude(j => j.Company)
                                            .FirstOrDefaultAsync(a => a.Id == appId);

            if (application == null)
                return NotFound();

            // Check if the logged-in user owns this job
            var userId = _userManager.GetUserId(User);
            if (application.Job.Company.OwnerUserId != userId)
                return Forbid();

            // Update status
            if (Enum.TryParse(status, out ApplicationStatus newStatus))
            {
                application.Status = newStatus;
                _context.Update(application);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Application status updated to {newStatus}.";
            }
            else
            {
                TempData["Error"] = "Invalid status.";
            }

            return RedirectToAction("ForJob", new { jobId = application.JobId });
        }

        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> ForJob(int jobId)
        {
            var userId = _userManager.GetUserId(User);

            // Check if this job belongs to the current employer
            var job = await _context.Jobs
                                    .Include(j => j.Company)
                                    .FirstOrDefaultAsync(j => j.Id == jobId && j.Company.OwnerUserId == userId);
            if (job == null)
                return NotFound();

            // Get all applications for this job
            var applications = await _context.JobApplications
                                             .Include(a => a.Applicant)
                                             .Where(a => a.JobId == jobId)
                                             .OrderByDescending(a => a.AppliedAt)
                                             .ToListAsync();

            ViewBag.JobTitle = job.Title;
            return View(applications);
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var app = await _context.JobApplications.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == userId);
            if (app == null)
            {
                TempData["Error"] = "Application not found.";
                return RedirectToAction("MyApplications");
            }

            if (!string.IsNullOrEmpty(app.ResumePath))
            {
                string fullPath = Path.Combine(_env.WebRootPath, app.ResumePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            _context.JobApplications.Remove(app);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been cancelled.";
            return RedirectToAction("MyApplications");
        }
    }
}
