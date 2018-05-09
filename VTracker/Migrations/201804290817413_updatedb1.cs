namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VisitPageActivity",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Activity = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        ScrollTop = c.Int(),
                        ScrollLeft = c.Int(),
                        MouseClickX = c.Int(),
                        MouseClickY = c.Int(),
                        ClickTagName = c.String(),
                        ClickTagId = c.String(),
                        visitpage_ID = c.Int(),
                        visit_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.VisitPage", t => t.visitpage_ID)
                .ForeignKey("dbo.Visit", t => t.visit_ID)
                .Index(t => t.visitpage_ID)
                .Index(t => t.visit_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.VisitPageActivity", new[] { "visit_ID" });
            DropIndex("dbo.VisitPageActivity", new[] { "visitpage_ID" });
            DropForeignKey("dbo.VisitPageActivity", "visit_ID", "dbo.Visit");
            DropForeignKey("dbo.VisitPageActivity", "visitpage_ID", "dbo.VisitPage");
            DropTable("dbo.VisitPageActivity");
        }
    }
}
