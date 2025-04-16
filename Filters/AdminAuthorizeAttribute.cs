using System.Web.Mvc;

namespace OnlineMusicStore.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var role = filterContext.HttpContext.Session["Role"];
            if (role == null || role.ToString() != "Admin")
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }
    }
}
