using System.Web.Mvc;

namespace TqCcnetDashboard.Controllers
{
    public class HomeController : AbstractController
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}