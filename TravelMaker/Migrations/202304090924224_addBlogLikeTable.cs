namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogLikeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogLikes",
                c => new
                    {
                        BlogLikeId = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BlogLikeId)
                .ForeignKey("dbo.Blogs", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogLikes", "BlogId", "dbo.Blogs");
            DropIndex("dbo.BlogLikes", new[] { "BlogId" });
            DropTable("dbo.BlogLikes");
        }
    }
}
