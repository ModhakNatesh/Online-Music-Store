using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using OnlineMusicStore.Filters;
using OnlineMusicStore.Models;
using System;
using System.Text;

namespace OnlineMusicStore.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private OnlineMusicStoreContext db = new OnlineMusicStoreContext();

        public ActionResult Index()
        {
            ViewBag.TotalUsers = db.Users.Count();
            ViewBag.TotalMusic = db.MusicItems.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalSales = db.Orders.SelectMany(o => o.OrderItems).Sum(i => (decimal?)i.Price * i.Quantity) ?? 0;
            return View();
        }

        public ActionResult MusicItems() => View(db.MusicItems.ToList());

        public ActionResult CreateMusic() => View();

        [HttpPost]
        public ActionResult CreateMusic(MusicItem item)
        {
            if (ModelState.IsValid)
            {
                db.MusicItems.Add(item);
                db.SaveChanges();
                return RedirectToAction("MusicItems");
            }
            return View(item);
        }

        public ActionResult EditMusic(int id) => View(db.MusicItems.Find(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMusic(MusicItem updatedItem)
        {
            var music = db.MusicItems.Find(updatedItem.MusicItemId);
            if (music == null) return HttpNotFound();

            int oldStock = music.Stock;

            music.Title = updatedItem.Title;
            music.Artist = updatedItem.Artist;
            music.Genre = updatedItem.Genre;
            music.Price = updatedItem.Price;
            music.ReleaseDate = updatedItem.ReleaseDate;
            music.Stock = updatedItem.Stock;
            music.ImageUrl = updatedItem.ImageUrl;

            db.SaveChanges();

            // 💌 Send back-in-stock notifications
            if (oldStock == 0 && updatedItem.Stock > 0)
            {
                NotifyWishListUsers(music.MusicItemId, music.Title);
            }

            TempData["Message"] = "Music item updated.";
            return RedirectToAction("MusicItems");
        }

        private void NotifyWishListUsers(int musicItemId, string title)
        {
            var users = db.WishListItems
                .Where(w => w.MusicItemId == musicItemId)
                .Select(w => w.User.Email)
                .Distinct()
                .ToList();

            foreach (var email in users)
            {
                string body = GenerateWishlistNotificationEmail(title, musicItemId);

                try
                {
                    OnlineMusicStore.Helpers.EmailHelper.SendOrderConfirmation(
                        email,
                        $"🎵 \"{title}\" is now back in stock!",
                        body);
                }
                catch (Exception ex)
                {
                    // Log or ignore if email fails
                    System.Diagnostics.Debug.WriteLine($"Failed to send email to {email}: {ex.Message}");
                }
            }
        }

        private string GenerateWishlistNotificationEmail(string title, int musicItemId)
        {
            StringBuilder sb = new StringBuilder();

            // Email Header
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.AppendLine("    <title>Your Wish List Item Is Back!</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("        .header { background-color: #4a6fe9; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }");
            sb.AppendLine("        .content { padding: 20px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }");
            sb.AppendLine("        .item-name { font-size: 18px; font-weight: bold; color: #333; }");
            sb.AppendLine("        .stock-alert { display: inline-block; background-color: #28a745; color: white; padding: 4px 12px; border-radius: 4px; font-weight: bold; }");
            sb.AppendLine("        .cta-button { display: inline-block; background-color: #4a6fe9; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 15px; }");
            sb.AppendLine("        .footer { margin-top: 30px; font-size: 12px; color: #777; text-align: center; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header Section
            sb.AppendLine("    <div class='header'>");
            sb.AppendLine("        <h1>🎵 Great News From Your Wish List!</h1>");
            sb.AppendLine("    </div>");

            // Content Section
            sb.AppendLine("    <div class='content'>");
            sb.AppendLine("        <p>Hello Music Lover,</p>");
            sb.AppendLine("        ");
            sb.AppendLine($"        <p>We're excited to let you know that <span class='item-name'>\"{title}\"</span> is now <span class='stock-alert'>BACK IN STOCK</span>!</p>");
            sb.AppendLine("        ");
            sb.AppendLine("        <p>This item from your wish list is available for purchase again. Based on past demand, we recommend securing your copy soon before it sells out again.</p>");
            sb.AppendLine("        ");
            sb.AppendLine("        <p style='text-align: center;'>");
            sb.AppendLine($"            <a href='https://yourmusicstore.com/Music/Details/{musicItemId}' class='cta-button'>View Item</a>");
            sb.AppendLine("        </p>");
            sb.AppendLine("        ");
            sb.AppendLine("        <p>Thank you for your patience and for being a valued customer of Online Music Store.</p>");
            sb.AppendLine("        ");
            sb.AppendLine("        <p>Happy listening!</p>");
            sb.AppendLine("        <p>The Online Music Store Team</p>");
            sb.AppendLine("    </div>");

            // Footer Section
            sb.AppendLine("    <div class='footer'>");
            sb.AppendLine("        <p>This email was sent because you added this item to your wish list. ");
            sb.AppendLine("        If you no longer wish to receive these notifications, you can manage your preferences in your account settings.</p>");
            sb.AppendLine($"        <p>&copy; {DateTime.Now.Year} Online Music Store. All rights reserved.</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        public ActionResult DeleteMusic(int id)
        {
            var item = db.MusicItems.Find(id);
            db.MusicItems.Remove(item);
            db.SaveChanges();
            return RedirectToAction("MusicItems");
        }

        public ActionResult Users() => View(db.Users.ToList());

        public ActionResult DeleteUser(int id)
        {
            var user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Users");
        }

        public ActionResult Orders()
        {
            // Disable tracking for better performance in read-only scenarios
            db.Configuration.ProxyCreationEnabled = false;

            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.MusicItem))
                .ToList();

            db.Configuration.ProxyCreationEnabled = true;

            return View(orders);
        }

        public ActionResult Feedbacks()
        {
            var feedbacks = db.Feedbacks.Include("User").OrderByDescending(f => f.SubmittedAt).ToList();
            return View(feedbacks);
        }

    }
}
