using System;
using System.Linq;
using System.Net;

namespace TqLib.Dashboard
{
    public class DashboardVersionChecker
    {
        public string CheckUrl { get; set; } = "https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/latest";

        public string GetRemoteVersion()
        {
            using (var wc = new CustomWebClient())
            {
                wc.DownloadString(CheckUrl);
                return wc.ResponseUri.Segments.Last();
            }
        }
    }

    public class PluginVersionChecker
    {
        public string CheckUrl { get; set; } = "https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/latest";

        public string GetRemoteVersion()
        {
            using (var wc = new CustomWebClient())
            {
                wc.DownloadString(CheckUrl);
                return wc.ResponseUri.Segments.Last();
            }
        }
    }

    internal class CustomWebClient : WebClient
    {
        public Uri ResponseUri { get; private set; }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            ResponseUri = response.ResponseUri;
            return response;
        }
    }
}