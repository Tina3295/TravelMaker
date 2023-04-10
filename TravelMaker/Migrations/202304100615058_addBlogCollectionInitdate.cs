namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBlogCollectionInitdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogCollections", "InitDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BlogCollections", "InitDate");
        }
    }
}
