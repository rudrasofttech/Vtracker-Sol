namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Members : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Member",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 250),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                        Status = c.Int(nullable: false),
                        PublicId = c.Guid(nullable: false),
                        AuthToken = c.Guid(nullable: false),
                        TokenCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Member");
        }
    }
}
