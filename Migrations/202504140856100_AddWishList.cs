namespace OnlineMusicStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWishList : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WishListItems",
                c => new
                    {
                        WishListItemId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        MusicItemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.WishListItemId)
                .ForeignKey("dbo.MusicItems", t => t.MusicItemId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.MusicItemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WishListItems", "UserId", "dbo.Users");
            DropForeignKey("dbo.WishListItems", "MusicItemId", "dbo.MusicItems");
            DropIndex("dbo.WishListItems", new[] { "MusicItemId" });
            DropIndex("dbo.WishListItems", new[] { "UserId" });
            DropTable("dbo.WishListItems");
        }
    }
}
