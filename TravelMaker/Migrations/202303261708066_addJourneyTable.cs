namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addJourneyTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Journeys",
                c => new
                    {
                        AttractionId = c.Int(nullable: false, identity: true),
                        JourneyName = c.String(maxLength: 30),
                        UserId = c.Int(nullable: false),
                        InitDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AttractionId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Journeys", "UserId", "dbo.Users");
            DropIndex("dbo.Journeys", new[] { "UserId" });
            DropTable("dbo.Journeys");
        }
    }
}
