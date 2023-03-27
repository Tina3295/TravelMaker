namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctImageKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attractions", "Attraction_AttractionId", "dbo.Attractions");
            DropIndex("dbo.Attractions", new[] { "Attraction_AttractionId" });
            DropColumn("dbo.Attractions", "Attraction_AttractionId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attractions", "Attraction_AttractionId", c => c.Int());
            CreateIndex("dbo.Attractions", "Attraction_AttractionId");
            AddForeignKey("dbo.Attractions", "Attraction_AttractionId", "dbo.Attractions", "AttractionId");
        }
    }
}
