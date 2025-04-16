using OnlineMusicStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OnlineMusicStore.Controllers
{
    public class HomeController : Controller
    {
        private OnlineMusicStoreContext db = new OnlineMusicStoreContext();

        public ActionResult Index()
        {
            if (Session["UserId"] != null)
                return RedirectToAction("UserHome", "Account");

            var musicItems = db.MusicItems.ToList();

            ViewBag.Genres = musicItems.Select(m => m.Genre).Distinct().ToList();

            return View(musicItems);
        }
    }
}
