namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedatabase : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.VisitPageActivity", "visitpage_ID", "dbo.VisitPage");
            DropForeignKey("dbo.VisitPageActivity", "visit_ID", "dbo.Visit");
            DropIndex("dbo.VisitPageActivity", new[] { "visitpage_ID" });
            DropIndex("dbo.VisitPageActivity", new[] { "visit_ID" });
            CreateTable(
                "dbo.VisitActivity",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Activity = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        MouseClickX = c.Int(),
                        MouseClickY = c.Int(),
                        ClickTagName = c.String(maxLength: 50),
                        ClickTagId = c.String(maxLength: 200),
                        visitpage_ID = c.Int(),
                        visit_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.VisitPage", t => t.visitpage_ID)
                .ForeignKey("dbo.Visit", t => t.visit_ID)
                .Index(t => t.visitpage_ID)
                .Index(t => t.visit_ID);
            
            DropTable("dbo.VisitPageActivity");
        }
        
        public override void Down()
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
                        ClickTagName = c.String(maxLength: 100),
                        ClickTagId = c.String(maxLength: 500),
                        visitpage_ID = c.Int(),
                        visit_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropIndex("dbo.VisitActivity", new[] { "visit_ID" });
            DropIndex("dbo.VisitActivity", new[] { "visitpage_ID" });
            DropForeignKey("dbo.VisitActivity", "visit_ID", "dbo.Visit");
            DropForeignKey("dbo.VisitActivity", "visitpage_ID", "dbo.VisitPage");
            DropTable("dbo.VisitActivity");
            CreateIndex("dbo.VisitPageActivity", "visit_ID");
            CreateIndex("dbo.VisitPageActivity", "visitpage_ID");
            AddForeignKey("dbo.VisitPageActivity", "visit_ID", "dbo.Visit", "ID");
            AddForeignKey("dbo.VisitPageActivity", "visitpage_ID", "dbo.VisitPage", "ID");
        }
    }
}
