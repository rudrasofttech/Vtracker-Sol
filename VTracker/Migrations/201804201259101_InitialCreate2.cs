namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Visit", "Webpage_ID", "dbo.Webpage");
            DropIndex("dbo.Visit", new[] { "Webpage_ID" });
            CreateTable(
                "dbo.VisitPage",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LastPingDate = c.DateTime(nullable: false),
                        visit_ID = c.Int(),
                        webpage_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Visit", t => t.visit_ID)
                .ForeignKey("dbo.Webpage", t => t.webpage_ID)
                .Index(t => t.visit_ID)
                .Index(t => t.webpage_ID);
            
            DropColumn("dbo.Visit", "MacAddress");
            DropColumn("dbo.Visit", "Webpage_ID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Visit", "Webpage_ID", c => c.Int(nullable: false));
            AddColumn("dbo.Visit", "MacAddress", c => c.String(nullable: false, maxLength: 25));
            DropIndex("dbo.VisitPage", new[] { "webpage_ID" });
            DropIndex("dbo.VisitPage", new[] { "visit_ID" });
            DropForeignKey("dbo.VisitPage", "webpage_ID", "dbo.Webpage");
            DropForeignKey("dbo.VisitPage", "visit_ID", "dbo.Visit");
            DropTable("dbo.VisitPage");
            CreateIndex("dbo.Visit", "Webpage_ID");
            AddForeignKey("dbo.Visit", "Webpage_ID", "dbo.Webpage", "ID", cascadeDelete: true);
        }
    }
}
