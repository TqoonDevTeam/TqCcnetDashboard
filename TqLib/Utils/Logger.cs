using log4net;

namespace TqLib.Utils
{
    public static class Log4netLogger
    {
        public static readonly ILog root = LogManager.GetLogger("default");
        public static readonly ILog msbuild = LogManager.GetLogger("msbuild");
        public static readonly ILog nuget = LogManager.GetLogger("nuget");
        public static readonly ILog nunit = LogManager.GetLogger("nunit");
    }
}