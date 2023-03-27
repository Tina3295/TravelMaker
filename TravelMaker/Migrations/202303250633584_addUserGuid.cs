namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "UserGuid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "UserGuid");
        }
    }
}
