using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        public string Title { get; set; }

        public string Description { get; set; }
        public string Location { get; set; }

        // Comma-separated skills
        public string Skills { get; set; }

        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }

        public string EmploymentType { get; set; }

        // ✅ Only CompanyId is required, not Company
        [Required(ErrorMessage = "Please select a company")]
        public int CompanyId { get; set; }

        public Company? Company { get; set; }  // ❌ remove any [Required] here!

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
