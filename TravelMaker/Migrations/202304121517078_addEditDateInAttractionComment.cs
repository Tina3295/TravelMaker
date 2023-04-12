namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEditDateInAttractionComment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttractionComments", "EditDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AttractionComments", "EditDate");
        }
    }
}
