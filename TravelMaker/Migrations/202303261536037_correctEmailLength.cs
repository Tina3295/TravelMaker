namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctEmailLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Attractions", "Email", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attractions", "Email", c => c.String(maxLength: 10));
        }
    }
}
