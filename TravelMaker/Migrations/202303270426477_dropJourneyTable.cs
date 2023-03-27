namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dropJourneyTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Journeys", "UserId", "dbo.Users");
            DropIndex("dbo.Journeys", new[] { "UserId" });
            DropTable("dbo.Journeys");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.AttractionId);
            
            CreateIndex("dbo.Journeys", "UserId");
            AddForeignKey("dbo.Journeys", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
    }
}
