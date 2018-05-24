namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbupdate1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.VisitActivity", "SecondsPassed", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.VisitActivity", "SecondsPassed", c => c.Int(nullable: false));
        }
    }
}
