namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTourLikeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TourLikes",
                c => new
                    {
                        TourLikeId = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.TourLikeId)
                .ForeignKey("dbo.Tours", t => t.TourId, cascadeDelete: true)
                .Index(t => t.TourId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TourLikes", "TourId", "dbo.Tours");
            DropIndex("dbo.TourLikes", new[] { "TourId" });
            DropTable("dbo.TourLikes");
        }
    }
}
