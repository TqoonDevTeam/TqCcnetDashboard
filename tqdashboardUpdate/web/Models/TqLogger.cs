using log4net;
using System.Linq;
using TqLib.Dashboard;

namespace TqCcnetDashboard
{
    public static class TqLogger
    {
        public static readonly ILog System = LogManager.GetLogger("system");
        public static readonly ILog Event = LogManager.GetLogger("event");

        public static CustomEventAppender CustomEventAppender
        {
            get
            {
                return Event.Logger.Repository.GetAppenders().OfType<CustomEventAppender>().First();
            }
        }
    }
}