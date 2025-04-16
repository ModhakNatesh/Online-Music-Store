using System.Linq;
using System.Web.Mvc;
using OnlineMusicStore.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnlineMusicStore.Controllers
{
    public class AccountController : Controller
    {
        private OnlineMusicStoreContext db = new OnlineMusicStoreContext();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password before saving
                user.Password = HashPassword(user.Password);

                db.Users.Add(user);
                db.SaveChanges();
                TempData["Success"] = "Registration successful!";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // Password hashing method
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user != null)
            {
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["Role"] = user.Role;
                TempData["LoginSuccess"] = "Welcome, " + user.FullName + "!";
                return RedirectToAction("UserHome");
            }

            ViewBag.LoginError = "Invalid username or password.";
            return View();
        }

        public ActionResult UserHome()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            ViewBag.Username = Session["Username"];

            var recentMusic = db.MusicItems
                .GroupBy(m => m.Genre)
                .ToDictionary(g => g.Key, g => g.Take(3).ToList()); // Top 3 per genre

            return View(recentMusic);
        }

        public new ActionResult Profile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(User updatedUser, string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);

            if (user == null)
            {
                // User not found in database
                TempData["ProfileError"] = "User profile not found.";
                return RedirectToAction("Login");
            }

            // Basic details update
            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.Address = updatedUser.Address;
            user.CreditCard = updatedUser.CreditCard;

            // Password change logic - only process if the user entered something in oldPassword field
            if (!string.IsNullOrWhiteSpace(oldPassword))
            {
                string hashedOld = HashPassword(oldPassword);

                // Check if old password is correct
                if (user.Password != hashedOld)
                {
                    ModelState.AddModelError("oldPassword", "Current password is incorrect.");
                    return View(user);
                }

                // If old password is correct but no new password was provided
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    ModelState.AddModelError("newPassword", "New password is required.");
                    return View(user);
                }

                // If confirm password is missing
                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ModelState.AddModelError("confirmPassword", "Please confirm your new password.");
                    return View(user);
                }

                // Check if new password and confirm password match
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("confirmPassword", "New passwords do not match.");
                    return View(user);
                }

                // Validate password strength
                if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("newPassword", "New password must be at least 6 characters.");
                    return View(user);
                }

                // Update the password
                user.Password = HashPassword(newPassword);
                TempData["ProfileSuccess"] = "Profile and password updated successfully!";
            }
            else
            {
                // Only update profile without password change
                TempData["ProfileSuccess"] = "Profile updated successfully!";
            }

            // Save changes to database
            db.SaveChanges();
            return View(user);
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


    }
}
