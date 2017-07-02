using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TqCcnetDashboard.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]

namespace TqCcnetDashboard
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}