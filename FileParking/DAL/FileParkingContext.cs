using FileParking.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
namespace FileParking.DAL
{

    public class FileParkingContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ParkedFile> ParkedFiles { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<Transfer> Transfers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

}