using System.Web.Mvc;
using Boki_ELearning.Helpers;

namespace Boki_ELearning.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session[SessionKeys.AccountId] != null)
            {
                var role = Session[SessionKeys.Role] as string;
                if (RoleHelper.IsAdmin(role))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Index", "Learning");
            }

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
