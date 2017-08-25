using NUnit.Framework;
using TqLib.Dashboard.CcnetPluginInstall;

namespace TqLib.zTest.ccnet.Local.Helper
{
    internal class CcnetPluginInstallerTest
    {
        [Test]
        //[Ignore("로컬전용")]
        public void Test()
        {
            string srcDirectory = @"C:\Program Files (x86)\CruiseControl.NET\imsi_test\TqLib";
            string pluginDirectory = @"C:\Program Files (x86)\CruiseControl.NET\plugin";
            string serviceDirecotry = @"C:\Program Files (x86)\CruiseControl.NET\server";
            var a = new CcnetPluginInstaller(srcDirectory, pluginDirectory, serviceDirecotry);
            a.Install();
        }
    }
}