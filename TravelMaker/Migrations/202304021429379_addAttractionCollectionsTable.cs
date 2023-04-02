namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAttractionCollectionsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttractionCollections",
                c => new
                    {
                        AttractionCollectionId = c.Int(nullable: false, identity: true),
                        AttractionId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AttractionCollectionId)
                .ForeignKey("dbo.Attractions", t => t.AttractionId, cascadeDelete: true)
                .Index(t => t.AttractionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttractionCollections", "AttractionId", "dbo.Attractions");
            DropIndex("dbo.AttractionCollections", new[] { "AttractionId" });
            DropTable("dbo.AttractionCollections");
        }
    }
}
