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

            modelBuilder.Entity<Webpage>()
                .Property(p => p.PathHash)
                .HasColumnType("binary(32)")
                .HasComputedColumnSql("CAST(HASHBYTES('SHA2_256', LOWER([Path]) + '?' + LOWER([QueryString])) AS binary(32))", true);

            modelBuilder.Entity<Visit>()
                .Property(v => v.IsBot)
                .HasDefaultValue(false);

            modelBuilder.Entity<Website>()
                .Property(w => w.TimeZone)
                .HasDefaultValue("India Standard Time");
        }
    }
}
