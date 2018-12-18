namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class memberchanged : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Member", "AuthToken", c => c.Guid());
            AlterColumn("dbo.Member", "TokenCreated", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Member", "TokenCreated", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Member", "AuthToken", c => c.Guid(nullable: false));
        }
    }
}
