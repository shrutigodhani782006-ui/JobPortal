using System;

namespace JobPortal.Models
{
    public class Internship
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Skills { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
