using System.Data.Entity;

namespace OnlineMusicStore.Models
{
    public class OnlineMusicStoreContext : DbContext
    {

        public OnlineMusicStoreContext() : base("OnlineMusicStoreDB") { }

        public DbSet<User> Users { get; set; }
        public DbSet<MusicItem> MusicItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<WishListItem> WishListItems { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Rating> Ratings { get; set; }


    }
}
