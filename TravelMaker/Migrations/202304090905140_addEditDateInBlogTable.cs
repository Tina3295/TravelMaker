namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEditDateInBlogTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blogs", "EditDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Blogs", "EditDate");
        }
    }
}
