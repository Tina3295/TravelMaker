namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctTelLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Attractions", "Tel", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attractions", "Tel", c => c.String(maxLength: 10));
        }
    }
}
