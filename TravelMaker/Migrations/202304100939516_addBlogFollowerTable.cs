namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogFollowerTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogFollowers",
                c => new
                    {
                        BlogFollowerId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        FollowingUserId = c.Int(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.BlogFollowerId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogFollowers", "UserId", "dbo.Users");
            DropIndex("dbo.BlogFollowers", new[] { "UserId" });
            DropTable("dbo.BlogFollowers");
        }
    }
}
