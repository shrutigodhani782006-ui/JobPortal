using System;
using Microsoft.AspNetCore.Identity;

namespace JobPortal.Models
{
    public enum ApplicationStatus
    {
        Applied,
        Shortlisted,
        Rejected,
        Hired
    }

    public class JobApplication
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; }

        public string ApplicantId { get; set; }
        public ApplicationUser Applicant { get; set; }

        public string? ResumePath { get; set; }   // ✅ Nullable now
        public string? CoverLetter { get; set; }  // ✅ Nullable now

        public DateTime AppliedOn { get; set; } = DateTime.Now;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
        public int? Experience { get; set; }
    }
}
