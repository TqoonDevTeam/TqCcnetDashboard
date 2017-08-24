using System.IO;
using System.Web;
using TqCcnetDashboard.Config;
using TqLib.ccnet.Local;
using TqLib.Dashboard;

namespace TqCcnetDashboard.Models
{
    public class DashboardUpdateManager
    {
        private object _lock = new object();
        private bool nowBusy = false;
        private DashboardUpdator DashboardUpdator;
        private PluginUpdator PluginUpdator;

        public DashboardUpdateManager()
        {
            DashboardUpdator = new DashboardUpdator
            {
                DashboardFolder = GetDashboardFolder(),
                DownloadFolder = GetDashboardDownloadFolder(),
                Logger = TqLogger.Event,
                SystemLogger = TqLogger.System
            };
            PluginUpdator = new PluginUpdator
            {
                PluginDirectory = CCNET.PluginDirectory,
                DownloadFolder = GetPluginDownloadFolder(),
                ServiceDirecotry = CCNET.ServiceDirectory,
                Logger = TqLogger.Event,
                SystemLogger = TqLogger.System
            };
        }

        public void Update()
        {
            if (!nowBusy)
            {
                lock (_lock)
                {
                    if (!nowBusy)
                    {
                        nowBusy = true;
                        SetDownLoadUrl();
                        DashboardUpdator.Update();
                        PluginUpdator.Update();
                        nowBusy = false;
                    }
                }
            }
        }

        private void SetDownLoadUrl()
        {
            var version = new DashboardVersionChecker().GetRemoteVersion();

            var dashboardUrl = ConfigManager.Get("DashboardUrl", "");
            var pluginUrl = ConfigManager.Get("PluginUrl", "");
            if (string.IsNullOrEmpty(dashboardUrl))
            {
                dashboardUrl = string.Format("https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/download/{0}/web.zip", version);
            }
            if (string.IsNullOrEmpty(pluginUrl))
            {
                pluginUrl = string.Format("https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/download/{0}/plugins.zip", version);
            }

            DashboardUpdator.DownloadUrl = dashboardUrl;
            PluginUpdator.DownloadUrl = pluginUrl;
        }

        private string GetDashboardFolder()
        {
            return MapPathUtil.MapPath("~/");
        }

        private string GetDashboardDownloadFolder()
        {
            return Path.Combine(new DirectoryInfo(MapPathUtil.MapPath("~/")).Parent.FullName, @"tqdashboardUpdate\web");
        }

        private string GetPluginDownloadFolder()
        {
            return Path.Combine(new DirectoryInfo(MapPathUtil.MapPath("~/")).Parent.FullName, @"tqdashboardUpdate\plugin");
        }
    }
}