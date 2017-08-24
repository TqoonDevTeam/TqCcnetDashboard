using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.Dashboard;

namespace TqCcnetDashboard.Models.SignalR
{
    [HubName("TqoonDevTeamMessageHub")]
    public class TqoonDevTeamMessageHub : Hub
    {
        private readonly TqoonDevTeamStation _TqoonDevTeamStation;

        public TqoonDevTeamMessageHub() : this(TqoonDevTeamStation.Instance)
        {
        }

        public TqoonDevTeamMessageHub(TqoonDevTeamStation tqoonDevTeamStation)
        {
            _TqoonDevTeamStation = tqoonDevTeamStation;
        }

        public IDictionary<string, ProjectStatus[]> getAllProjectStatus()
        {
            return _TqoonDevTeamStation.getAllProjectStatus();
        }

        public object GetSystemUpdate()
        {
            return _TqoonDevTeamStation.GetSystemUpdate();
        }
    }

    public class TqoonDevTeamStation
    {
        private readonly static Lazy<TqoonDevTeamStation> _instance = new Lazy<TqoonDevTeamStation>(() => new TqoonDevTeamStation(GlobalHost.ConnectionManager.GetHubContext<TqoonDevTeamMessageHub>().Clients));
        public static TqoonDevTeamStation Instance { get { return _instance.Value; } }

        private IHubConnectionContext<dynamic> Clients { get; set; }

        private TqoonDevTeamStation(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            CurrentSite.ProjectStatusChanged += CurrentSite_ProjectStatusChanged;
            TqLogger.CustomEventAppender.ProcessChanged += CustomEventAppender_ProcessChanged;
        }

        private void CustomEventAppender_ProcessChanged(object sender, TqLib.Dashboard.CustomEventAppenderArgs e)
        {
            BroadcastSystemUpdate(e);
        }

        private void CurrentSite_ProjectStatusChanged(object sender, ProjectStatusChangedArgs e)
        {
            BroadcastProjectStatus();
        }

        private void BroadcastProjectStatus()
        {
            Clients.All.updateProjectStatus(CurrentSite.ProjectStatusCollection);
        }

        private void BroadcastSystemUpdate(CustomEventAppenderArgs e)
        {
            Clients.All.systemUpdate(e);
        }

        public IDictionary<string, ProjectStatus[]> getAllProjectStatus()
        {
            return CurrentSite.ProjectStatusCollection;
        }

        public object GetSystemUpdate()
        {
            return new CustomEventAppenderArgs { Msg = "", Level = "" };
        }
    }
}