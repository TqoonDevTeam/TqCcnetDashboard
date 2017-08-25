using NUnit.Framework;
using TqLib.Dashboard.CcnetPluginInstall;

namespace TqLib.zTest.ccnet.Local.Helper
{
    internal class AssemblyBindingTest
    {
        private AssemblyBinding item;

        [SetUp]
        public void SetUp()
        {
            item = new AssemblyBinding();
        }

        [Test]
        [Ignore("로컬전용")]
        public void Load()
        {
            string path = @"D:\Test\ccnet.exe.config";
            item.Load(path);
            item.Save();
        }
    }
}