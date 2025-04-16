using OnlineMusicStore.Models;
using System.Web.Mvc;

namespace OnlineMusicStore.Controllers
{
    public class FeedbackController : Controller
    {
        private OnlineMusicStoreContext db = new OnlineMusicStoreContext();

        public ActionResult Submit()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(Feedback feedback)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                feedback.UserId = (int)Session["UserId"];
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                TempData["Success"] = "Thank you for your feedback!";
                return RedirectToAction("Submit");
            }

            return View(feedback);
        }
    }
}
