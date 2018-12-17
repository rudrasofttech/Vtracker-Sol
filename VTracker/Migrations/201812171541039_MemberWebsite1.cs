namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MemberWebsite1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MemberWebsiteRelation",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Role = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                        Member_ID = c.Int(),
                        Website_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Member", t => t.Member_ID)
                .ForeignKey("dbo.Website", t => t.Website_ID)
                .Index(t => t.Member_ID)
                .Index(t => t.Website_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemberWebsiteRelation", "Website_ID", "dbo.Website");
            DropForeignKey("dbo.MemberWebsiteRelation", "Member_ID", "dbo.Member");
            DropIndex("dbo.MemberWebsiteRelation", new[] { "Website_ID" });
            DropIndex("dbo.MemberWebsiteRelation", new[] { "Member_ID" });
            DropTable("dbo.MemberWebsiteRelation");
        }
    }
}
