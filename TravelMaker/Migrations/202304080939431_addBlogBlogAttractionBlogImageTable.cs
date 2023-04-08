namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogBlogAttractionBlogImageTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        BlogId = c.Int(nullable: false, identity: true),
                        BlogGuid = c.String(),
                        UserId = c.Int(nullable: false),
                        Title = c.String(maxLength: 30),
                        Category = c.String(),
                        Cover = c.String(),
                        Status = c.Int(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.BlogAttractions",
                c => new
                    {
                        BlogAttractionId = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        AttractionId = c.Int(nullable: false),
                        Content = c.String(maxLength: 1000),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogAttractionId)
                .ForeignKey("dbo.Blogs", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.BlogImages",
                c => new
                    {
                        BlogImageId = c.Int(nullable: false, identity: true),
                        BlogAttractionId = c.Int(nullable: false),
                        ImageName = c.String(),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogImageId)
                .ForeignKey("dbo.BlogAttractions", t => t.BlogAttractionId, cascadeDelete: true)
                .Index(t => t.BlogAttractionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Blogs", "UserId", "dbo.Users");
            DropForeignKey("dbo.BlogImages", "BlogAttractionId", "dbo.BlogAttractions");
            DropForeignKey("dbo.BlogAttractions", "BlogId", "dbo.Blogs");
            DropIndex("dbo.BlogImages", new[] { "BlogAttractionId" });
            DropIndex("dbo.BlogAttractions", new[] { "BlogId" });
            DropIndex("dbo.Blogs", new[] { "UserId" });
            DropTable("dbo.BlogImages");
            DropTable("dbo.BlogAttractions");
            DropTable("dbo.Blogs");
        }
    }
}
