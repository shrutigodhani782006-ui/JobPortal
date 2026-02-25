using JobPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Internship> Internships { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Job → Company (FK required, but Company navigation optional)
            builder.Entity<Job>()
                   .HasOne(j => j.Company)
                   .WithMany()
                   .HasForeignKey(j => j.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(); // enforce CompanyId, not Company object

            // Internship → Company (same as Job)
            builder.Entity<Internship>()
                   .HasOne(i => i.Company)
                   .WithMany()
                   .HasForeignKey(i => i.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

            // JobApplication → Job
            builder.Entity<JobApplication>()
                   .HasOne(a => a.Job)
                   .WithMany()
                   .HasForeignKey(a => a.JobId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

            // JobApplication → Applicant (ApplicationUser)
            builder.Entity<JobApplication>()
                   .HasOne(a => a.Applicant)
                   .WithMany()
                   .HasForeignKey(a => a.ApplicantId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();
        }
    }
}
