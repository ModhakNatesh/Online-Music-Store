namespace OnlineMusicStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStockToMusicItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MusicItems", "Stock", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MusicItems", "Stock");
        }
    }
}
