using System.Web.Mvc;
using TqCcnetDashboard.Quartz;
using TqLib.ccnet.Local;

namespace TqCcnetDashboard.Controllers
{
    public class DashboardController : AbstractController
    {
        // GET: Dashboard
        public JsonResult GetProjectStatus()
        {
            using (var ccnet = new CCNET())
            {
                var list = ccnet.Server.GetProjectStatus();
                return Json(list);
            }
        }

        public JsonResult ForceBuild(string projectName, string host)
        {
            using (var ccnet = new CCNET(host))
            {
                ccnet.Server.ForceBuild(projectName);
            }
            QuartzTqoonDevTeamCiServerJobManager.TriggerImmediately();
            return Json();
        }

        public JsonResult AbortBuild(string projectName, string host)
        {
            using (var ccnet = new CCNET(host))
            {
                ccnet.Server.AbortBuild(projectName);
            }
            QuartzTqoonDevTeamCiServerJobManager.TriggerImmediately();
            return Json();
        }
    }
}