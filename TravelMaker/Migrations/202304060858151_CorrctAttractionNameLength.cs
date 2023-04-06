namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrctAttractionNameLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Attractions", "AttractionName", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attractions", "AttractionName", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
