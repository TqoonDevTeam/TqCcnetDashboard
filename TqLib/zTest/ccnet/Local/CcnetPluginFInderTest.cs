using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var types = finder.GetPluginTypes();
        }
    }
}
