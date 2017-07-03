using log4net;

namespace TqCcnetDashboard
{
    public static class TqLogger
    {
        public static readonly ILog Web = LogManager.GetLogger("web");
    }
}