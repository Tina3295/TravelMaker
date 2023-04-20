namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserPermission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Permission", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Permission");
        }
    }
}
