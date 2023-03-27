namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCategoryCategoryAttractionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CategoryAttractions",
                c => new
                    {
                        CategoryAttractionId = c.Int(nullable: false, identity: true),
                        AttractionId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryAttractionId)
                .ForeignKey("dbo.Attractions", t => t.AttractionId, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.AttractionId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CategoryAttractions", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.CategoryAttractions", "AttractionId", "dbo.Attractions");
            DropIndex("dbo.CategoryAttractions", new[] { "CategoryId" });
            DropIndex("dbo.CategoryAttractions", new[] { "AttractionId" });
            DropTable("dbo.Categories");
            DropTable("dbo.CategoryAttractions");
        }
    }
}
