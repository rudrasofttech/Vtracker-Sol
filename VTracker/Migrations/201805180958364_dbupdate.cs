namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbupdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VisitActivity", "SecondsPassed", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.VisitActivity", "SecondsPassed");
        }
    }
}
