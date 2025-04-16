using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlineMusicStore.Models;

namespace OnlineMusicStore.Controllers
{
    public class WishListController : Controller
    {
        private OnlineMusicStoreContext db = new OnlineMusicStoreContext();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];
            var wishList = db.WishListItems
                             .Where(w => w.UserId == userId)
                             .ToList();

            return View(wishList);
        }

        public ActionResult Add(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];
            if (!db.WishListItems.Any(w => w.MusicItemId == id && w.UserId == userId))
            {
                db.WishListItems.Add(new WishListItem
                {
                    UserId = userId,
                    MusicItemId = id
                });
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Music");
        }

        public ActionResult Remove(int id)
        {
            var item = db.WishListItems.Find(id);
            if (item != null)
            {
                db.WishListItems.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult MoveToCart(int id)
        {
            var item = db.WishListItems.Include("MusicItem").FirstOrDefault(w => w.WishListItemId == id);
            if (item != null)
            {
                List<CartItem> cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
                var existing = cart.FirstOrDefault(c => c.MusicItem.MusicItemId == item.MusicItemId);

                if (existing != null)
                    existing.Quantity++;
                else
                    cart.Add(new CartItem { MusicItem = item.MusicItem, Quantity = 1 });

                Session["Cart"] = cart;

                db.WishListItems.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Cart", "Music");
        }
    }
}
