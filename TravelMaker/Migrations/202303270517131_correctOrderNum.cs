namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctOrderNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TourAttractions", "OrderNum", c => c.Int(nullable: false));
            DropColumn("dbo.TourAttractions", "Order");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TourAttractions", "Order", c => c.Int(nullable: false));
            DropColumn("dbo.TourAttractions", "OrderNum");
        }
    }
}
