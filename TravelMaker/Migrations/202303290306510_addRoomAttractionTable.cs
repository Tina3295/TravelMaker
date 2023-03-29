namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRoomAttractionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RoomAttractions",
                c => new
                    {
                        RoomAttractionId = c.Int(nullable: false, identity: true),
                        RoomId = c.Int(nullable: false),
                        AttractionId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RoomAttractionId)
                .ForeignKey("dbo.Attractions", t => t.AttractionId, cascadeDelete: true)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .Index(t => t.RoomId)
                .Index(t => t.AttractionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoomAttractions", "RoomId", "dbo.Rooms");
            DropForeignKey("dbo.RoomAttractions", "AttractionId", "dbo.Attractions");
            DropIndex("dbo.RoomAttractions", new[] { "AttractionId" });
            DropIndex("dbo.RoomAttractions", new[] { "RoomId" });
            DropTable("dbo.RoomAttractions");
        }
    }
}
