namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTourAttractionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TourAttractions",
                c => new
                    {
                        TourAttractionId = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        AttractionId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TourAttractionId)
                .ForeignKey("dbo.Attractions", t => t.AttractionId, cascadeDelete: true)
                .ForeignKey("dbo.Tours", t => t.TourId, cascadeDelete: true)
                .Index(t => t.TourId)
                .Index(t => t.AttractionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TourAttractions", "TourId", "dbo.Tours");
            DropForeignKey("dbo.TourAttractions", "AttractionId", "dbo.Attractions");
            DropIndex("dbo.TourAttractions", new[] { "AttractionId" });
            DropIndex("dbo.TourAttractions", new[] { "TourId" });
            DropTable("dbo.TourAttractions");
        }
    }
}
