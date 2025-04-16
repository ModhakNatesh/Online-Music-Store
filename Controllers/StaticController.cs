using System.Net.Mail;
using System.Web.Mvc;
using System.Net;
using OnlineMusicStore.Helpers;

namespace OnlineMusicStore.Controllers
{
    public class StaticController : Controller
    {
        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs(string name, string email, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "All fields are required.";
                return View();
            }

            string body = $@"
                <h3>New Contact Message</h3>
                <p><strong>Name:</strong> {name}</p>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Message:</strong><br />{message}</p>
            ";

            try
            {
                EmailHelper.SendOrderConfirmation("modhaknateshtoo@gmail.com", "New Contact Message", body);
                TempData["Success"] = "Your message has been sent!";
            }
            catch
            {
                TempData["Error"] = "There was a problem sending your message. Please try again later.";
            }

            return View();
        }
    }
}
