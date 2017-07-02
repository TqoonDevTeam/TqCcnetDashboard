using NUnit.Framework;
using TqLib.ccnet.Local.Helper;

namespace TqLib.zTest.ccnet.Local.Helper
{
    public class CcnetServiceConfigUpdatorTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var updator = new CcnetServiceConfigUpdator(@"C:\Program Files (x86)\CruiseControl.NET\server");
            updator.Update();
        }
    }
}