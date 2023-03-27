namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addImageTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        ImageId = c.Int(nullable: false, identity: true),
                        AttractionId = c.Int(nullable: false),
                        ImageName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ImageId)
                .ForeignKey("dbo.Attractions", t => t.AttractionId, cascadeDelete: true)
                .Index(t => t.AttractionId);
            
            AddColumn("dbo.Attractions", "Attraction_AttractionId", c => c.Int());
            CreateIndex("dbo.Attractions", "Attraction_AttractionId");
            AddForeignKey("dbo.Attractions", "Attraction_AttractionId", "dbo.Attractions", "AttractionId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Images", "AttractionId", "dbo.Attractions");
            DropForeignKey("dbo.Attractions", "Attraction_AttractionId", "dbo.Attractions");
            DropIndex("dbo.Images", new[] { "AttractionId" });
            DropIndex("dbo.Attractions", new[] { "Attraction_AttractionId" });
            DropColumn("dbo.Attractions", "Attraction_AttractionId");
            DropTable("dbo.Images");
        }
    }
}
