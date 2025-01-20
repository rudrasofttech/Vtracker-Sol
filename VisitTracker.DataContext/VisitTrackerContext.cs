using Microsoft.EntityFrameworkCore;
using VisitTracker.Models;

namespace VisitTracker.DataContext
{
    public class VisitTrackerDBContext : DbContext
    {
        public VisitTrackerDBContext(DbContextOptions<VisitTrackerDBContext> options) : base(options)
        {
        }

        public DbSet<Visit> Visits { get; set; }
        public DbSet<VisitPage> VisitPages { get; set; }
        public DbSet<VisitActivity> VisitActivities { get; set; }
        public DbSet<Webpage> Webpages { get; set; }
        public DbSet<VisitPage> Pages { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<IP2Location> IP2Locations { get; set; }
        public DbSet<VisitCountView> VisitCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Entity<Visit>().ToTable("Visit");
            modelBuilder.Entity<VisitActivity>().ToTable("VisitActivity");
            modelBuilder.Entity<Webpage>().ToTable("Webpage");
            modelBuilder.Entity<VisitPage>().ToTable("VisitPage");
            modelBuilder.Entity<Website>().ToTable("Website");
            modelBuilder.Entity<IP2Location>().ToTable("IP2Location");
            modelBuilder.Entity<VisitCountView>().ToView("VisitCountView").HasNoKey();
        }
    }
}
