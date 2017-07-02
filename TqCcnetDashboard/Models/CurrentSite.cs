using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.ccnet.Local;
using TqLib.ccnet.Local.Helper;

namespace TqCcnetDashboard
{
    public static class CurrentSite
    {
        public static event EventHandler<ProjectStatusChangedArgs> ProjectStatusChanged;

        public static readonly TqCcnetDashboardUpdator Updator, ExternalPluginUpdator;
        public static IDictionary<string, ProjectStatus[]> ProjectStatusCollection { get; set; }
        public static IList<ProjectStatus> AllProjectStatus { get { return ProjectStatusCollection == null ? new List<ProjectStatus>() : ProjectStatusCollection.Values.SelectMany(t => t).OrderBy(t => t.ServerName).ThenBy(t => t.Name).ToList(); } }

        static CurrentSite()
        {
            Updator = new TqCcnetDashboardUpdator()
            {
                DashboardFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/"),
                DownloadFolder = Path.Combine(new DirectoryInfo(System.Web.Hosting.HostingEnvironment.MapPath("~/")).Parent.FullName, "tqdashboardUpdate"),
                PluginFolder = CCNET.PluginDirectory,
                ServiceFolder = CCNET.ServiceDirectory
            };
            ExternalPluginUpdator = new TqCcnetDashboardUpdator()
            {
                DashboardFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/"),
                DownloadFolder = Path.Combine(new DirectoryInfo(System.Web.Hosting.HostingEnvironment.MapPath("~/")).Parent.FullName, "pluginTemp"),
                PluginFolder = CCNET.PluginDirectory,
                ServiceFolder = CCNET.ServiceDirectory
            };
            ProjectStatusCollection = new Dictionary<string, ProjectStatus[]>();
        }

        public static void OnProjectChagned(string host)
        {
            ProjectStatusChanged?.Invoke(null, new ProjectStatusChangedArgs() { Host = host });
        }
    }

    public class ProjectStatusChangedArgs : EventArgs
    {
        public string Host { get; set; }
    }
}