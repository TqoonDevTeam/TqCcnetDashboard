using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TqCcnetDashboard.Config;
using TqCcnetDashboard.Web.Mvc;
using TqLib.ccnet.Core;
using TqLib.ccnet.Local;
using TqLib.Dashboard;

namespace TqCcnetDashboard.Controllers
{
    public class SystemSettingController : AbstractController
    {
        [ViewDownloadFilter]
        public ActionResult Wizard(string id)
        {
            ViewData["showDownloadBtn"] = Request.Params["toFile"] == null;

            using (var ccnet = new CCNET())
            {
                var xmlConfig = ccnet.Server.GetProject(id);
                var jObject = ProjectWebJsonConverter.SerializedProjectToWebJObjectConverter.Convert(xmlConfig);
                ViewData["jObject"] = jObject.ToString(Newtonsoft.Json.Formatting.None);
                return View();
            }
        }

        public JsonResult GetServerCheckInformation()
        {
            var serverVersion = typeof(SystemSettingController).Assembly.GetName().Version.ToString();
            var pluginVersion = typeof(CCNET).Assembly.GetName().Version.ToString();
            var externalPlugins = new CcnetPluginFInder(CCNET.ServiceDirectory, CCNET.PluginDirectory).GetExternalPluginsInfo();
            var dashboardUrl = ConfigManager.Get("DashboardUrl", "");
            var pluginUrl = ConfigManager.Get("PluginUrl", "");
            return Json(new
            {
                serverVersion,
                pluginVersion,
                externalPlugins,
                dashboardUrl,
                pluginUrl
            });
        }

        public void SystemUpdate()
        {
            CurrentSite.Updator.Update();
        }

        public JsonResult GetRemoteVersion()
        {
            return Json(new DashboardVersionChecker().GetRemoteVersion());
        }

        [AllowCrossDomainFilter]
        public JsonResult ProjectRegFromWizard(string value)
        {
            string json = $"{{ \"project\":{value} }}";
            using (var ccnet = new CCNET())
            {
                JObject project = JObject.Parse(json);
                string projectName = project["project"]["name"].Value<string>();
                var xmlConfig = ProjectWebJsonConverter.WebJObjectToSerializedProjectConverter.Convert(project);
                if (ccnet.Server.GetProjectStatus().Any(t => t.Name == projectName))
                {
                    return Json(new { error = true, msg = "같은 이름의 프로젝트가 이미 등록되었습니다." });
                }
                else
                {
                    ccnet.Server.AddProject(xmlConfig);
                    ccnet.WaitAddComplete(projectName);
                }
            }
            return Json(new { error = false, value });
        }

        public JsonResult PluginUpload()
        {
            if (CurrentSite.Updator.NowBusy)
            {
                return Json(new { error = true, msg = "다른 설치가 진행 중 입니다." });
            }
            else
            {
                CurrentSite.Updator.CleanUpPluginDownloadFolder();
                var file = Request.Files[0];
                var filePath = Path.Combine(CurrentSite.Updator.GetPluginDownloadFolder(), file.FileName);
                file.SaveAs(filePath);
                CurrentSite.Updator.UpdatePluginOnly(filePath);
            }
            return Json();
        }
    }
}