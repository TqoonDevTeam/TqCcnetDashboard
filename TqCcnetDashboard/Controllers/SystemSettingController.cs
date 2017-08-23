using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TqCcnetDashboard.Web.Mvc;
using TqLib.ccnet.Core;
using TqLib.ccnet.Local;
using TqLib.ccnet.Local.Helper;

namespace TqCcnetDashboard.Controllers
{
    public class SystemSettingController : AbstractController
    {
        public JsonResult CheckEnvironmentVariable()
        {
            var checker = new EnviromentVariableChecker();
            return Json(checker.Check());
        }

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
            var pluginInstalled = System.IO.File.Exists(Path.Combine(CCNET.PluginDirectory, CcnetPluginFInder.TqDashboardDefaultPluginFileName));
            //var externalPlugins = new CcnetPluginFInder(CCNET.ServiceDirectory).GetExternalPluginsWithInfo();
            return Json(new
            {
                serverVersion,
                pluginVersion,
                pluginInstalled,
              //  externalPlugins
            });
        }

        public void SystemUpdate()
        {
            CurrentSite.Updator.Update();
        }

        public JsonResult GetRemoteVersion()
        {
            return Json(CurrentSite.Updator.GetRemoteVersion());
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
            if (!CurrentSite.ExternalPluginUpdator.IsBysy || CurrentSite.Updator.IsBysy)
            {
                CurrentSite.ExternalPluginUpdator.CheckDownloadFolder();

                var file = Request.Files[0];
                var filePath = Path.Combine(CurrentSite.ExternalPluginUpdator.DownloadFolder, file.FileName);
                file.SaveAs(filePath);

                CurrentSite.ExternalPluginUpdator.UpdateExternalPlugin(filePath);
            }
            else
            {
                return Json(new { error = true, msg = "다른 설치가 진행 중 입니다." });
            }
            return Json();
        }
    }
}