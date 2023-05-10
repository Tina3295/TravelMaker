namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNotificationAndTypeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        Status = c.Boolean(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        Sender = c.Int(nullable: false),
                        Receiver = c.Int(nullable: false),
                        NotificationTypeId = c.Int(nullable: false),
                        InitDate = c.DateTime(),
                        RoomGuid = c.String(),
                        OldRoomName = c.String(maxLength: 30),
                        NewRoomName = c.String(maxLength: 30),
                        AddVoteDate = c.String(maxLength: 10),
                        BlogGuid = c.String(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.NotificationTypes", t => t.NotificationTypeId, cascadeDelete: true)
                .Index(t => t.NotificationTypeId);
            
            CreateTable(
                "dbo.NotificationTypes",
                c => new
                    {
                        NotificationTypeId = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.NotificationTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "NotificationTypeId", "dbo.NotificationTypes");
            DropIndex("dbo.Notifications", new[] { "NotificationTypeId" });
            DropTable("dbo.NotificationTypes");
            DropTable("dbo.Notifications");
        }
    }
}
