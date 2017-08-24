using System.Web.Mvc;

namespace TqCcnetDashboard.Controllers
{
    //[Authorize]
    public class HomeController : AbstractController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}