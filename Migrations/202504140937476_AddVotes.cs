namespace OnlineMusicStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVotes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        VoteId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        MusicItemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VoteId)
                .ForeignKey("dbo.MusicItems", t => t.MusicItemId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.MusicItemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Votes", "MusicItemId", "dbo.MusicItems");
            DropIndex("dbo.Votes", new[] { "MusicItemId" });
            DropIndex("dbo.Votes", new[] { "UserId" });
            DropTable("dbo.Votes");
        }
    }
}
