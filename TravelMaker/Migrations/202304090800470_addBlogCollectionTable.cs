namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogCollectionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogCollections",
                c => new
                    {
                        BlogCollectionId = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BlogCollectionId)
                .ForeignKey("dbo.Blogs", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogCollections", "BlogId", "dbo.Blogs");
            DropIndex("dbo.BlogCollections", new[] { "BlogId" });
            DropTable("dbo.BlogCollections");
        }
    }
}
