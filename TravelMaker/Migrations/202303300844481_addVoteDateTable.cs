namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addVoteDateTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VoteDates",
                c => new
                    {
                        VoteDateId = c.Int(nullable: false, identity: true),
                        RoomId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VoteDateId)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RoomId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VoteDates", "UserId", "dbo.Users");
            DropForeignKey("dbo.VoteDates", "RoomId", "dbo.Rooms");
            DropIndex("dbo.VoteDates", new[] { "UserId" });
            DropIndex("dbo.VoteDates", new[] { "RoomId" });
            DropTable("dbo.VoteDates");
        }
    }
}
