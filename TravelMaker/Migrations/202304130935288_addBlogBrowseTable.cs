namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogBrowseTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogBrowses",
                c => new
                    {
                        BlogBrowseId = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogBrowseId)
                .ForeignKey("dbo.Blogs", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            AddColumn("dbo.Blogs", "PageViewCounts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogBrowses", "BlogId", "dbo.Blogs");
            DropIndex("dbo.BlogBrowses", new[] { "BlogId" });
            DropColumn("dbo.Blogs", "PageViewCounts");
            DropTable("dbo.BlogBrowses");
        }
    }
}
