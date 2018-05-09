namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Visit", "ScreenWidth", c => c.Int());
            AddColumn("dbo.Visit", "ScreenHeight", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Visit", "ScreenHeight");
            DropColumn("dbo.Visit", "ScreenWidth");
        }
    }
}
