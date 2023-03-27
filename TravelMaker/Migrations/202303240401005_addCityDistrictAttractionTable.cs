namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCityDistrictAttractionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Attractions",
                c => new
                    {
                        AttractionId = c.Int(nullable: false, identity: true),
                        AttractionName = c.String(nullable: false, maxLength: 30),
                        Introduction = c.String(),
                        OpenStatus = c.Boolean(nullable: false),
                        DistrictId = c.Int(nullable: false),
                        Address = c.String(maxLength: 100),
                        Tel = c.String(maxLength: 10),
                        Email = c.String(maxLength: 10),
                        Elong = c.Decimal(nullable: false, precision: 11, scale: 8),
                        Nlat = c.Decimal(nullable: false, precision: 10, scale: 8),
                        Location = c.Geography(),
                        OfficialSite = c.String(maxLength: 300),
                        Facebook = c.String(maxLength: 300),
                        OpenTime = c.String(maxLength: 150),
                        InitDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.AttractionId)
                .ForeignKey("dbo.Districts", t => t.DistrictId, cascadeDelete: true)
                .Index(t => t.DistrictId);
            
            CreateTable(
                "dbo.Districts",
                c => new
                    {
                        DistrictId = c.Int(nullable: false, identity: true),
                        DistrictName = c.String(nullable: false, maxLength: 10),
                        CityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DistrictId)
                .ForeignKey("dbo.Cities", t => t.CityId, cascadeDelete: true)
                .Index(t => t.CityId);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        CityId = c.Int(nullable: false, identity: true),
                        CittyName = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.CityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Districts", "CityId", "dbo.Cities");
            DropForeignKey("dbo.Attractions", "DistrictId", "dbo.Districts");
            DropIndex("dbo.Districts", new[] { "CityId" });
            DropIndex("dbo.Attractions", new[] { "DistrictId" });
            DropTable("dbo.Cities");
            DropTable("dbo.Districts");
            DropTable("dbo.Attractions");
        }
    }
}
