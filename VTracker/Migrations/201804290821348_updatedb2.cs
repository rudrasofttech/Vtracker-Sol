namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.VisitPageActivity", "ClickTagName", c => c.String(maxLength: 100));
            AlterColumn("dbo.VisitPageActivity", "ClickTagId", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.VisitPageActivity", "ClickTagId", c => c.String());
            AlterColumn("dbo.VisitPageActivity", "ClickTagName", c => c.String());
        }
    }
}
