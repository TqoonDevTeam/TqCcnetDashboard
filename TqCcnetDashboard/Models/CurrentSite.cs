using System;
using System.Collections.Generic;
using System.Linq;
using ThoughtWorks.CruiseControl.Remote;
using TqCcnetDashboard.Models;
using TqLib.Dashboard;

namespace TqCcnetDashboard
{
    public static class CurrentSite
    {
        public static event EventHandler<ProjectStatusChangedArgs> ProjectStatusChanged;

        public static readonly DashboardUpdateManager Updator;

        public static IDictionary<string, ProjectStatus[]> ProjectStatusCollection { get; set; }

        public static IList<ProjectStatus> AllProjectStatus { get { return ProjectStatusCollection == null ? new List<ProjectStatus>() : ProjectStatusCollection.Values.SelectMany(t => t).OrderBy(t => t.ServerName).ThenBy(t => t.Name).ToList(); } }

        static CurrentSite()
        {
            Updator = new DashboardUpdateManager();
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