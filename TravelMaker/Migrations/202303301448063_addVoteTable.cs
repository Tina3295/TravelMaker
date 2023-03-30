namespace TravelMaker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addVoteTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        VoteId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        VoteDateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VoteId)
                .ForeignKey("dbo.VoteDates", t => t.VoteDateId, cascadeDelete: true)
                .Index(t => t.VoteDateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "VoteDateId", "dbo.VoteDates");
            DropIndex("dbo.Votes", new[] { "VoteDateId" });
            DropTable("dbo.Votes");
        }
    }
}
