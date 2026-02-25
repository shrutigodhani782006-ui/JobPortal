using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class JobApplicationViewModel
    {
        public int JobId { get; set; }

        [Required]
        public string CoverLetter { get; set; }

        [Required]
        [Range(0, 50)]
        public int Experience { get; set; }

        [Required]
        public IFormFile ResumeFile { get; set; }
    }
}
