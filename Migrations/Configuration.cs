namespace OnlineMusicStore.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<OnlineMusicStore.Models.OnlineMusicStoreContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(OnlineMusicStore.Models.OnlineMusicStoreContext context)
        {
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var admin = new OnlineMusicStore.Models.User
                {
                    Username = "admin",
                    Password = HashPassword("admin123"), 
                    Role = "Admin",
                    FullName = "Administrator",
                    Email = "modhaknateshtoo@gmail.com",
                    Address = "123 Admin",
                    CreditCard = "1234-5678-9312-2345"
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }


        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

    }
}
