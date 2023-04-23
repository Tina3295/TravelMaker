namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DecimalSet : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Attractions", "Elong", c => c.Decimal(nullable: false, precision: 11, scale: 8));
            AlterColumn("dbo.Attractions", "Nlat", c => c.Decimal(nullable: false, precision: 10, scale: 8));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attractions", "Nlat", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Attractions", "Elong", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
