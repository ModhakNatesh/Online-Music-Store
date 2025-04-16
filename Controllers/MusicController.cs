using OnlineMusicStore.Helpers;
using OnlineMusicStore.Models;
using OnlineMusicStore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace OnlineMusicStore.Controllers
{
    public class MusicController : Controller
    {
        private OnlineMusicStoreContext db = new OnlineMusicStoreContext();

        public ActionResult Index()
        {
            var items = db.MusicItems.ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult Vote(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];

            var alreadyVoted = db.Votes.Any(v => v.UserId == userId && v.MusicItemId == id);
            if (!alreadyVoted)
            {
                db.Votes.Add(new Vote
                {
                    UserId = userId,
                    MusicItemId = id
                });
                db.SaveChanges();
            }

            return RedirectToAction("ChartToppers");
        }

        public ActionResult ChartToppers()
        {
            var items = db.MusicItems.ToList();
            var votes = db.Votes.ToList();

            var model = items.Select(item => new ChartTopperViewModel
            {
                MusicItem = item,
                VoteCount = votes.Count(v => v.MusicItemId == item.MusicItemId)
            })
            .OrderByDescending(x => x.VoteCount)
            .Take(10)
            .ToList();

            ViewBag.Voted = Session["UserId"] != null
                ? votes.Where(v => v.UserId == (int)Session["UserId"]).Select(v => v.MusicItemId).ToList()
                : new List<int>();

            return View(model);
        }

        public ActionResult Search(string query, string genre, string sortBy, decimal? minPrice, decimal? maxPrice)
        {
            var results = db.MusicItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                results = results.Where(m => m.Title.Contains(query) || m.Artist.Contains(query) || m.Genre.Contains(query));

            if (!string.IsNullOrWhiteSpace(genre))
                results = results.Where(m => m.Genre == genre);

            if (minPrice.HasValue)
                results = results.Where(m => m.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                results = results.Where(m => m.Price <= maxPrice.Value);

            // Sorting
            switch (sortBy)
            {
                case "price_asc":
                    results = results.OrderBy(m => m.Price);
                    break;
                case "price_desc":
                    results = results.OrderByDescending(m => m.Price);
                    break;
                case "date_desc":
                    results = results.OrderByDescending(m => m.ReleaseDate);
                    break;
                case "popular":
                    results = results.OrderByDescending(m => m.Votes.Count());
                    break;
            }

            ViewBag.Query = query;
            ViewBag.Genres = db.MusicItems.Select(m => m.Genre).Distinct().ToList();
            ViewBag.CurrentGenre = genre;
            ViewBag.SortBy = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(results.ToList());
        }

        public ActionResult Rate(int musicItemId)
        {
            int userId = (int)Session["UserId"];

            var hasOrdered = db.Orders.Any(o =>
                o.UserId == userId &&
                o.OrderItems.Any(oi => oi.MusicItemId == musicItemId));

            if (!hasOrdered) return RedirectToAction("OrderHistory");

            var existing = db.Ratings.FirstOrDefault(r => r.UserId == userId && r.MusicItemId == musicItemId);

            return View(existing ?? new Rating { MusicItemId = musicItemId, UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rate(Rating rating)
        {
            if (rating.Stars < 1 || rating.Stars > 5)
            {
                ModelState.AddModelError("Stars", "Rating must be between 1 and 5.");
            }

            var existing = db.Ratings.FirstOrDefault(r => r.UserId == rating.UserId && r.MusicItemId == rating.MusicItemId);

            if (existing != null)
            {
                existing.Stars = rating.Stars;
                existing.Review = rating.Review;
            }
            else
            {
                db.Ratings.Add(rating);
            }

            db.SaveChanges();
            return RedirectToAction("OrderHistory");
        }


        public ActionResult LatestReleases()
        {
            DateTime lastMonth = DateTime.Now.AddDays(-30);
            var recentSongs = db.MusicItems
                .Where(m => m.ReleaseDate >= lastMonth)
                .OrderByDescending(m => m.ReleaseDate)
                .ToList();

            return View(recentSongs);
        }


        public ActionResult AddToCart(int id)
        {
            if (Session["UserId"] == null)
            {
                TempData["LoginRequired"] = "Please log in to add items to your cart!";
                return RedirectToAction("Login", "Account");
            }

            var item = db.MusicItems.Find(id);
            if (item == null)
                return HttpNotFound();

            List<CartItem> cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            var existing = cart.FirstOrDefault(i => i.MusicItem.MusicItemId == id);
            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new CartItem { MusicItem = item, Quantity = 1 });

            Session["Cart"] = cart;

            TempData["Toast"] = $"{item.Title} added to cart!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                TempData["LoginRequired"] = "Please log in to add items to your cart!";
                return RedirectToAction("Login", "Account");
            }

            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                TempData["CartEmpty"] = "Cart is empty!";
                return RedirectToAction("Cart");
            }

            int userId = (int)Session["UserId"];
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderItems = cart.Select(c => new OrderItem
                {
                    MusicItemId = c.MusicItem.MusicItemId,
                    Quantity = c.Quantity,
                    Price = c.MusicItem.Price
                }).ToList()
            };

            db.Orders.Add(order);
            db.SaveChanges();

            Session["Cart"] = null;
            TempData["OrderSuccess"] = "Order placed successfully!";
            return RedirectToAction("OrderHistory");
        }


        public ActionResult Cart()
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            return View(cart);
        }

        public ActionResult Buy()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
                return RedirectToAction("Cart");

            ViewBag.Total = cart.Sum(i => i.MusicItem.Price * i.Quantity);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(string cardNumber, string expiry, string cvv)
        {
            if (Session["UserId"] == null || Session["Cart"] == null)
                return RedirectToAction("Login", "Account");

            var cart = Session["Cart"] as List<CartItem>;
            int userId = (int)Session["UserId"];

            // Simulate payment validation
            if (cardNumber.Length != 16 || cvv.Length != 3)
            {
                TempData["PaymentError"] = "Invalid payment details.";
                return RedirectToAction("Buy");
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            // Check stock and reduce it
            foreach (var cartItem in cart)
            {
                // Reduce stock
                var dbItem = db.MusicItems.Find(cartItem.MusicItem.MusicItemId);
                if (dbItem != null && dbItem.Stock >= cartItem.Quantity)
                {
                    dbItem.Stock -= cartItem.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        MusicItemId = cartItem.MusicItem.MusicItemId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.MusicItem.Price
                    });
                }
                else
                {
                    // Handle insufficient stock
                    TempData["PaymentError"] = $"Sorry, '{cartItem.MusicItem.Title}' is out of stock or has insufficient quantity.";
                    return RedirectToAction("Buy");
                }
            }

            db.Orders.Add(order);
            db.SaveChanges();
            var user = db.Users.Find(userId);
            string emailBody = GenerateOrderEmail(order);
            EmailHelper.SendOrderConfirmation(user.Email, "Your Order #" + order.OrderId + " Confirmation", emailBody);
            Session["Cart"] = null;
            return RedirectToAction("OrderSuccess", new { id = order.OrderId });
        }

        private string GenerateOrderEmail(Order order)
        {
            StringBuilder sb = new StringBuilder();
            decimal orderTotal = order.OrderItems.Sum(item => item.Price * item.Quantity);

            // Email Header
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='UTF-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }");
            sb.AppendLine("        .header { background-color: #f8f9fa; padding: 20px; border-bottom: 2px solid #dee2e6; }");
            sb.AppendLine("        .content { padding: 20px; }");
            sb.AppendLine("        .order-info { margin-bottom: 20px; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            sb.AppendLine("        th { background-color: #f8f9fa; text-align: left; }");
            sb.AppendLine("        th, td { padding: 10px; border: 1px solid #dee2e6; }");
            sb.AppendLine("        .total-row { font-weight: bold; background-color: #f8f9fa; }");
            sb.AppendLine("        .footer { margin-top: 30px; padding-top: 15px; border-top: 1px solid #dee2e6; font-size: 14px; color: #6c757d; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header Section
            sb.AppendLine("    <div class='header'>");
            sb.AppendLine("        <h2>Thank you for your order! 🎵</h2>");
            sb.AppendLine("    </div>");

            // Content Section
            sb.AppendLine("    <div class='content'>");
            sb.AppendLine("        <div class='order-info'>");
            sb.AppendLine($"            <p><strong>Order ID:</strong> #{order.OrderId}</p>");
            sb.AppendLine($"            <p><strong>Order Date:</strong> {order.OrderDate.ToString("MMMM d, yyyy")}</p>");
            sb.AppendLine("        </div>");

            // Order Table
            sb.AppendLine("        <table>");
            sb.AppendLine("            <thead>");
            sb.AppendLine("                <tr>");
            sb.AppendLine("                    <th>Item</th>");
            sb.AppendLine("                    <th>Quantity</th>");
            sb.AppendLine("                    <th>Price</th>");
            sb.AppendLine("                    <th>Subtotal</th>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("            </thead>");
            sb.AppendLine("            <tbody>");

            // Order Items
            foreach (var item in order.OrderItems)
            {
                decimal itemTotal = item.Price * item.Quantity;
                sb.AppendLine("                <tr>");
                sb.AppendLine($"                    <td>{item.MusicItem.Title}</td>");
                sb.AppendLine($"                    <td>{item.Quantity}</td>");
                sb.AppendLine($"                    <td>₹{item.Price:N2}</td>");
                sb.AppendLine($"                    <td>₹{itemTotal:N2}</td>");
                sb.AppendLine("                </tr>");
            }

            // Order Total
            sb.AppendLine("                <tr class='total-row'>");
            sb.AppendLine("                    <td colspan='3'>Total</td>");
            sb.AppendLine($"                    <td>₹{orderTotal:N2}</td>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("            </tbody>");
            sb.AppendLine("        </table>");

            // Footer
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p>If you have any questions about your order, please reply to this email or contact our support team.</p>");
            sb.AppendLine("            <p>Thank you for shopping with Online Music Store!</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }


        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderItems.MusicItem").FirstOrDefault(o => o.OrderId == id);
            return View(order);
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = Session["Cart"] as List<CartItem>;
            var item = cart?.FirstOrDefault(i => i.MusicItem.MusicItemId == id);
            if (item != null) cart.Remove(item);

            Session["Cart"] = cart;
            return RedirectToAction("Cart");
        }

        public ActionResult OrderHistory()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];

            var orders = db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

    }

    public class CartItem
    {
        public MusicItem MusicItem { get; set; }
        public int Quantity { get; set; }
    }

}
