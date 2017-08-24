using System.Web;

namespace TqCcnetDashboard
{
    public class MapPathUtil
    {
        public static string MapPath(string path)
        {
            if (HttpContext.Current == null)
            {
                return System.Web.Hosting.HostingEnvironment.MapPath(path);
            }
            else
            {
                return HttpContext.Current.Server.MapPath(path);
            }
        }
    }
}