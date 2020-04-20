namespace VTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Memberupdate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Member", "AuthToken");
            DropColumn("dbo.Member", "TokenCreated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Member", "TokenCreated", c => c.DateTime());
            AddColumn("dbo.Member", "AuthToken", c => c.Guid());
        }
    }
}
