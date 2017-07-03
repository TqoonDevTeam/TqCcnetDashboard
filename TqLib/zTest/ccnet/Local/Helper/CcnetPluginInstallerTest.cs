using NUnit.Framework;
using TqLib.ccnet.Local.Helper;

namespace TqLib.zTest.ccnet.Local.Helper
{
    internal class CcnetPluginInstallerTest
    {
        [Test]
        public void Test()
        {
            string srcDirectory = @"C:\Program Files (x86)\CruiseControl.NET\plugin";
            string pluginDirectory = "";
            string serviceDirecotry = "";
            var a = new CcnetPluginInstaller(srcDirectory, pluginDirectory, serviceDirecotry);
            a.Install();
        }
    }
}