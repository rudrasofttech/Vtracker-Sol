namespace VTracker.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using VTracker.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<VTracker.Models.VisitTrackerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(VTracker.Models.VisitTrackerContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //var websites = new List<Website> {
            //    new Website { ClientID = 1, Name = "Rockying.com", Status= RecordStatus.Active, DateCreated = DateTime.UtcNow },
            //    new Website { ClientID = 1, Name = "indiabobbles.com", Status= RecordStatus.Active, DateCreated = DateTime.UtcNow },
            //    new Website { ClientID = 1, Name = "rudrasofttech.com", Status= RecordStatus.Active, DateCreated = DateTime.UtcNow }
            //};

            //websites.ForEach(w => context.Websites.AddOrUpdate(p => p.Name, w));
            //context.SaveChanges();
        }
    }
}
