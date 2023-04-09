namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogCommentTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogComments",
                c => new
                    {
                        BlogCommentId = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Comment = c.String(nullable: false, maxLength: 500),
                        Status = c.Boolean(nullable: false),
                        InitDate = c.DateTime(),
                        EditDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogCommentId)
                .ForeignKey("dbo.Blogs", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogComments", "BlogId", "dbo.Blogs");
            DropIndex("dbo.BlogComments", new[] { "BlogId" });
            DropTable("dbo.BlogComments");
        }
    }
}
