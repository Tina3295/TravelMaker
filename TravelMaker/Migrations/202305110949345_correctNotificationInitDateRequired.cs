namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctNotificationInitDateRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Notifications", "InitDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notifications", "InitDate", c => c.DateTime());
        }
    }
}
