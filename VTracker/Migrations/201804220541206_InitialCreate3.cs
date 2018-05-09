namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Visit", "ClientCookie", c => c.Guid(nullable: false));
            AddColumn("dbo.Visit", "Website_ID", c => c.Int());
            AddForeignKey("dbo.Visit", "Website_ID", "dbo.Website", "ID");
            CreateIndex("dbo.Visit", "Website_ID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Visit", new[] { "Website_ID" });
            DropForeignKey("dbo.Visit", "Website_ID", "dbo.Website");
            DropColumn("dbo.Visit", "Website_ID");
            DropColumn("dbo.Visit", "ClientCookie");
        }
    }
}
