namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Website",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        ClientID = c.Int(),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Webpage",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Path = c.String(nullable: false, maxLength: 1024),
                        QueryString = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                        Status = c.Int(nullable: false),
                        Website_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Website", t => t.Website_ID, cascadeDelete: true)
                .Index(t => t.Website_ID);
            
            CreateTable(
                "dbo.Visit",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IPAddress = c.String(nullable: false, maxLength: 20),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                        Status = c.Int(nullable: false),
                        CountryAbbr = c.String(nullable: false, maxLength: 5),
                        Referer = c.String(nullable: false),
                        MacAddress = c.String(nullable: false, maxLength: 25),
                        WebsiteVisitorReferenceID = c.String(nullable: false, maxLength: 50),
                        LastPingDate = c.DateTime(nullable: false),
                        Webpage_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Webpage", t => t.Webpage_ID, cascadeDelete: true)
                .Index(t => t.Webpage_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Visit", new[] { "Webpage_ID" });
            DropIndex("dbo.Webpage", new[] { "Website_ID" });
            DropForeignKey("dbo.Visit", "Webpage_ID", "dbo.Webpage");
            DropForeignKey("dbo.Webpage", "Website_ID", "dbo.Website");
            DropTable("dbo.Visit");
            DropTable("dbo.Webpage");
            DropTable("dbo.Website");
        }
    }
}
