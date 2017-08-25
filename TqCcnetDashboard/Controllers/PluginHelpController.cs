using System.Linq;
using System.Web.Mvc;
using TqLib.ccnet.Local;

namespace TqCcnetDashboard.Controllers
{
    public class PluginHelpController : AbstractController
    {
        public JsonResult GetTaskPlugins()
        {
            return Json(CCNET.TaskPluginsReflectorTypes.Select(t => t.PluginName));
        }
    }
}