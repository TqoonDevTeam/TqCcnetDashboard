using NUnit.Framework;
using TqLib.ccnet.Local.Helper;

namespace TqLib.zTest.ccnet.Local
{
    public class CcnetServiceFinderTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var finder = new CcnetServiceFinder();
            var path = finder.FindServiceDirectory();
        }
    }
}