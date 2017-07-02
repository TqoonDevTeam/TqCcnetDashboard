using Moq;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using TqLib.ccnet.Core.Tasks;

namespace TqLib.zTest.ccnet.Core.Tasks
{
    [TestFixture(Category = "ccnet.core.Tasks")]
    public class TqHostsTaskTest
    {
        [Test]
        public void HostsCollectionTest()
        {
            string hosts = "# test\n\n127.0.0.1 local.ccnet\n127.0.0.5\n\n";

            var collection = new TqHostsTask.HostsCollection(hosts);

            Assert.AreEqual("127.0.0.1", collection["local.ccnet"], "값 꺼내기");

            collection["myhost.com"] = "255.255.255.0";
            Assert.AreEqual("255.255.255.0", collection["myhost.com"], "값 넣기");
        }

        [Test]
        [Ignore("로컬전용")]
        public void HostsTask()
        {
            var mock = new Mock<IIntegrationResult>();
            mock.Setup(t => t.BuildProgressInformation).Returns(new ThoughtWorks.CruiseControl.Core.Util.BuildProgressInformation("", ""));

            TqHostsTask task = new TqHostsTask();
            task.Ip = "127.0.0.1";
            task.Host = "myHostsTask.test";
            task.Run(mock.Object);

            Assert.AreEqual(true, task.WasSuccessful);
        }
    }
}