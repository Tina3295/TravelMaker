namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRoomGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "RoomGuid", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Rooms", "RoomGuid");
        }
    }
}
