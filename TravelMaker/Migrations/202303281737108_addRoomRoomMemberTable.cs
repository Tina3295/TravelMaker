namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRoomRoomMemberTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RoomMembers",
                c => new
                    {
                        RoomMemberId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoomId = c.Int(nullable: false),
                        Permission = c.Int(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.RoomMemberId)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoomId);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        RoomId = c.Int(nullable: false, identity: true),
                        RoomName = c.String(maxLength: 30),
                        Status = c.Boolean(nullable: false),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.RoomId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoomMembers", "UserId", "dbo.Users");
            DropForeignKey("dbo.RoomMembers", "RoomId", "dbo.Rooms");
            DropIndex("dbo.RoomMembers", new[] { "RoomId" });
            DropIndex("dbo.RoomMembers", new[] { "UserId" });
            DropTable("dbo.Rooms");
            DropTable("dbo.RoomMembers");
        }
    }
}
