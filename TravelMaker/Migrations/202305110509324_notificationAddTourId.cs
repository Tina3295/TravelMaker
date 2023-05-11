namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notificationAddTourId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "TourId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "TourId");
        }
    }
}
