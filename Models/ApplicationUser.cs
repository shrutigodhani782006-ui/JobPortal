using Microsoft.AspNetCore.Identity;

namespace JobPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? College { get; set; }   // note the ? 
        public bool IsEmployer { get; set; }
    }

}
