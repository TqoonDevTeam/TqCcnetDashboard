using NUnit.Framework;
using TqLib.ccnet.Local.Helper;

namespace TqLib.zTest.ccnet.Local.Helper
{
    public class TqCcnetDashboardUpdatorTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var updator = new TqCcnetDashboardUpdator()
            {
                DownloadFolder = @"D:\TEST\Download",
                DashboardFolder = @"D:\TEST\TqDashboard",
                PluginFolder = @"D:\TEST\TqPlugins",
                ServiceFolder = @"D:\TEST\TqService"
            };

            updator.Update();
        }
    }
}