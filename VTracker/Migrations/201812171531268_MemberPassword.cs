namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MemberPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Member", "Password", c => c.String(nullable: false, maxLength: 250));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Member", "Password");
        }
    }
}
