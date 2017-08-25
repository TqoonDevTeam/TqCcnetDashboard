using NUnit.Framework;
using TqLib.ccnet.Utils;

namespace TqLib.zTest.ccnet.Local.Helper
{
    public class CcnetServiceConfigUpdatorTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var updator = new CcnetServiceConfigInitializer(@"C:\Program Files (x86)\CruiseControl.NET\server");
            updator.Initialize();
        }
    }
}