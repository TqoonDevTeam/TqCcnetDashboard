using NUnit.Framework;
using TqLib.ccnet.Local.Helper;

namespace TqLib.zTest.ccnet.Local
{
    public class CcnetPluginFInderTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var finder = new CcnetPluginFInder(@"C:\Program Files (x86)\CruiseControl.NET\server");
            var types = finder.GetPluginTypeInfo();
        }
    }
}