namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class permissionBoolToInt : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Permission", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Permission", c => c.Boolean(nullable: false));
        }
    }
}
