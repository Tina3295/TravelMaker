namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrectDescribeToDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogAttractions", "Description", c => c.String(maxLength: 1000));
            DropColumn("dbo.BlogAttractions", "Describe");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BlogAttractions", "Describe", c => c.String(maxLength: 1000));
            DropColumn("dbo.BlogAttractions", "Description");
        }
    }
}
