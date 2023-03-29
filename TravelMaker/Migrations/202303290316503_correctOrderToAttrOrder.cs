namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctOrderToAttrOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoomAttractions", "AttrOrder", c => c.Int(nullable: false));
            DropColumn("dbo.RoomAttractions", "Order");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RoomAttractions", "Order", c => c.Int(nullable: false));
            DropColumn("dbo.RoomAttractions", "AttrOrder");
        }
    }
}
