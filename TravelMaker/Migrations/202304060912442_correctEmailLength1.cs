namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctEmailLength1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Attractions", "Email", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attractions", "Email", c => c.String(maxLength: 50));
        }
    }
}
