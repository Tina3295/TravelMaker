namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAttractionCommentTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttractionComments",
                c => new
                    {
                        AttractionCommentId = c.Int(nullable: false, identity: true),
                        AttractionId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Comment = c.String(maxLength: 500),
                        Score = c.Int(nullable: false),
                        Status = c.Boolean(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.AttractionCommentId)
                .ForeignKey("dbo.Attractions", t => t.AttractionId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.AttractionId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttractionComments", "UserId", "dbo.Users");
            DropForeignKey("dbo.AttractionComments", "AttractionId", "dbo.Attractions");
            DropIndex("dbo.AttractionComments", new[] { "UserId" });
            DropIndex("dbo.AttractionComments", new[] { "AttractionId" });
            DropTable("dbo.AttractionComments");
        }
    }
}
