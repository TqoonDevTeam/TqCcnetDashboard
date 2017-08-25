using System;
using System.IO;
using TqCcnetDashboard.Config;
using TqLib.ccnet.Local;
using TqLib.Dashboard;

namespace TqCcnetDashboard.Models
{
    public class DashboardUpdateManager
    {
        private object _lock = new object();
        public bool NowBusy = false;
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
            if (!NowBusy)
            {
                lock (_lock)
                {
                    if (!NowBusy)
                    {
                        NowBusy = true;
                        try
                        {
                            TqLogger.Event.Warn("SystemUpdate Start");
                            SetDownLoadUrl();
                            DashboardUpdator.Update();
                            PluginUpdator.Update();
                            TqLogger.Event.Warn("SystemUpdate Complete");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            NowBusy = false;
                        }
                    }
                }
            }
        }

        public void UpdatePluginOnly(string downloadedPath)
        {
            if (!NowBusy)
            {
                lock (_lock)
                {
                    if (!NowBusy)
                    {
                        NowBusy = true;
                        try
                        {
                            TqLogger.Event.Warn("SystemUpdate Start");
                            TqLogger.Event.Warn("PluginUpdate Only");
                            PluginUpdator.DownloadUrl = downloadedPath;
                            PluginUpdator.Update();
                            TqLogger.Event.Warn("SystemUpdate Complete");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            NowBusy = false;
                        }
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

        public string GetDashboardDownloadFolder()
        {
            return Path.Combine(new DirectoryInfo(MapPathUtil.MapPath("~/")).Parent.FullName, @"tqdashboardUpdate\web");
        }

        public string GetPluginDownloadFolder()
        {
            return Path.Combine(new DirectoryInfo(MapPathUtil.MapPath("~/")).Parent.FullName, @"tqdashboardUpdate\plugin");
        }

        public void CleanUpPluginDownloadFolder()
        {
            var path = GetPluginDownloadFolder();
            if (Directory.Exists(path)) Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}