using Spring.Core.CDH.Autowire;
using System.Web.Mvc;
using TqCcnetDashboard.Web.Mvc;

namespace TqCcnetDashboard
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    [TqoonDevTeamExceptionFilter]
    public abstract class AbstractController : Controller
    {
        public AbstractController()
        {
            SpringAutowire.Autowire(this);
        }

        protected JsonResult Json()
        {
            return Json(new { });
        }

        new protected JsonResult Json(object data)
        {
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        new protected JsonResult Json(object data, JsonRequestBehavior behavior)
        {
            var result = new NewtonJsonResult();
            result.Data = data;
            result.JsonRequestBehavior = behavior;
            if (Request.Browser.Type.StartsWith("IE"))
            {
                result.ContentType = "text/plain";
            }
            return result;
        }
    }
}