using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using TqLib.ccnet.Core.Tasks;

namespace TqLib.zTest.ccnet.core.Tasks
{
    [TestFixture(Category = "ccnet.core.Tasks")]
    public class TqIISTaskTest
    {
        [Test]
        public void ConfigTest()
        {
            var obj = NetReflector.Read(xml) as TqIISTask;
        }

        [Test]
        [Ignore("로컬전용")]
        public void RunTest()
        {
            var mock = new Mock<IIntegrationResult>();
            mock.Setup(t => t.BuildProgressInformation).Returns(new ThoughtWorks.CruiseControl.Core.Util.BuildProgressInformation("", ""));
            mock.Setup(t => t.AddTaskResult(It.IsAny<string>()));

            var task = NetReflector.Read(xml) as TqIISTask;
            task.Run(mock.Object);

            Assert.AreEqual(true, task.WasSuccessful);
        }

        private string xml = @"
<TqIIS>
    <siteName>테스트사이트</siteName>
    <poolName>TESTPOOL</poolName>
    <physicalPath>D:\TqoonDevTeamRepo\dev\AdprintWeb\AdprintWeb</physicalPath>
    <siteConfig></siteConfig>
    <poolConfig/>
    <bindings>*,local2.adprint.web,80
*,local2.adprint.web,443,‎8670368c030d846045193ca5e1c5124584733dfe
</bindings>
    <virtualDirectories>/Provider=C:\DEV\Provider\DEV_AdprintNewDB
/App_Data/TEST=C:\DEV\Provider\DEV_AdprintNewDB
</virtualDirectories>
</TqIIS>
";
    }
}