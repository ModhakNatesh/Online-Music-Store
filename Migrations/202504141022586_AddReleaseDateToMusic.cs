namespace OnlineMusicStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReleaseDateToMusic : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MusicItems", "ReleaseDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MusicItems", "ReleaseDate");
        }
    }
}
