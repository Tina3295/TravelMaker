namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogReplyTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogReplies",
                c => new
                    {
                        BlogReplyId = c.Int(nullable: false, identity: true),
                        BlogCommentId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Reply = c.String(nullable: false, maxLength: 500),
                        Status = c.Boolean(nullable: false),
                        InitDate = c.DateTime(),
                        EditDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogReplyId)
                .ForeignKey("dbo.BlogComments", t => t.BlogCommentId, cascadeDelete: true)
                .Index(t => t.BlogCommentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogReplies", "BlogCommentId", "dbo.BlogComments");
            DropIndex("dbo.BlogReplies", new[] { "BlogCommentId" });
            DropTable("dbo.BlogReplies");
        }
    }
}
