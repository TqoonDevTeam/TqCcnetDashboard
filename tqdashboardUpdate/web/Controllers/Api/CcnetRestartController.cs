using System.IO;
using TqLib.ccnet.Local;

namespace TqCcnetDashboard.Controllers.Api
{
    public class CcnetRestartController : AbstractApiController
    {
        public string Get()
        {
            using (var ccnet = new CCNET())
            {
                ccnet.StopService();
                //ccnet.UpdateServerConfig();
                //ccnet.UpdatePlugins(GetReal_savePath());
                ccnet.StartService();
            }
            return "success";
        }

        private string GetReal_savePath()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath("~/plugin");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}