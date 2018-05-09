namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate31 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Visit", "BrowserName", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.VisitPage", "BrowserWidth", c => c.Int());
            AddColumn("dbo.VisitPage", "BrowserHeight", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VisitPage", "BrowserHeight");
            DropColumn("dbo.VisitPage", "BrowserWidth");
            DropColumn("dbo.Visit", "BrowserName");
        }
    }
}
