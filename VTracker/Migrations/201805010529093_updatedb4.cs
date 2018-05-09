namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb4 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.VisitPage", "DocumentHeight");
            DropColumn("dbo.VisitPage", "DocumentWidth");
        }
        
        public override void Down()
        {
            AddColumn("dbo.VisitPage", "DocumentWidth", c => c.Int());
            AddColumn("dbo.VisitPage", "DocumentHeight", c => c.Int());
        }
    }
}
