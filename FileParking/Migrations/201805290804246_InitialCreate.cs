namespace FileParking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.User",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Email = c.String(nullable: false),
                        Password = c.String(nullable: false, maxLength: 300),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        Folder = c.String(nullable: false, maxLength: 500),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ParkedFile",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        FileName = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                        User_ID = c.Guid(),
                        Transfer_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.User", t => t.User_ID)
                .ForeignKey("dbo.Transfer", t => t.Transfer_ID)
                .Index(t => t.User_ID)
                .Index(t => t.Transfer_ID);
            
            CreateTable(
                "dbo.Recipient",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 250),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Transfer",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Message = c.String(),
                        Subject = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateRead = c.DateTime(),
                        Sender_ID = c.Guid(),
                        Recipient_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.User", t => t.Sender_ID)
                .ForeignKey("dbo.Recipient", t => t.Recipient_ID)
                .Index(t => t.Sender_ID)
                .Index(t => t.Recipient_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Transfer", new[] { "Recipient_ID" });
            DropIndex("dbo.Transfer", new[] { "Sender_ID" });
            DropIndex("dbo.ParkedFile", new[] { "Transfer_ID" });
            DropIndex("dbo.ParkedFile", new[] { "User_ID" });
            DropForeignKey("dbo.Transfer", "Recipient_ID", "dbo.Recipient");
            DropForeignKey("dbo.Transfer", "Sender_ID", "dbo.User");
            DropForeignKey("dbo.ParkedFile", "Transfer_ID", "dbo.Transfer");
            DropForeignKey("dbo.ParkedFile", "User_ID", "dbo.User");
            DropTable("dbo.Transfer");
            DropTable("dbo.Recipient");
            DropTable("dbo.ParkedFile");
            DropTable("dbo.User");
        }
    }
}
