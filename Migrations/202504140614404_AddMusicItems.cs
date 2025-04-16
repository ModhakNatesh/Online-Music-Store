namespace OnlineMusicStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMusicItems : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MusicItems",
                c => new
                    {
                        MusicItemId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Artist = c.String(nullable: false),
                        Genre = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ImageUrl = c.String(),
                    })
                .PrimaryKey(t => t.MusicItemId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MusicItems");
        }
    }
}
