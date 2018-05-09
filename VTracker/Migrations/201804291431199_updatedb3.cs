namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Visit", "LastVisitID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Visit", "LastVisitID");
        }
    }
}
