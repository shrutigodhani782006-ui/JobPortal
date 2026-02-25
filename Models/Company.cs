using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Location { get; set; }

        // This should NOT be validated from UI
        [ScaffoldColumn(false)] // hides it from scaffolding
        public string? OwnerUserId { get; set; }
    }
}
