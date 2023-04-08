namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renameContentToDescribe : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogAttractions", "Describe", c => c.String(maxLength: 1000));
            DropColumn("dbo.BlogAttractions", "Content");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BlogAttractions", "Content", c => c.String(maxLength: 1000));
            DropColumn("dbo.BlogAttractions", "Describe");
        }
    }
}
