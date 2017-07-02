using System;
using System.Linq;
using System.Net;

namespace TqLib.ccnet.Local.Helper
{
    public class TqDashboardRemoteVersionChecker
    {
        public string GetRemoteVersion()
        {
            using (var wc = new CustomWebClient())
            {
                wc.DownloadString("https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/latest");
                return wc.ResponseUri.Segments.Last();
            }
        }

        private class CustomWebClient : WebClient
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
}