namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VisitPage", "ScrollTop", c => c.Int());
            AddColumn("dbo.VisitPage", "ScrollLeft", c => c.Int());
            AddColumn("dbo.VisitPage", "DocumentHeight", c => c.Int());
            AddColumn("dbo.VisitPage", "DocumentWidth", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.VisitPage", "DocumentWidth");
            DropColumn("dbo.VisitPage", "DocumentHeight");
            DropColumn("dbo.VisitPage", "ScrollLeft");
            DropColumn("dbo.VisitPage", "ScrollTop");
        }
    }
}
