using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using ThoughtWorks.CruiseControl.Core;
using TqLib.ccnet.Core.Tasks;

namespace TqLib.zTest.ccnet.Core.Tasks
{
    [TestFixture(Category = "ccnet.core.Tasks")]
    public class TqRsyncTaskTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void RunTest()
        {
            var mock = new Mock<IIntegrationResult>();
            mock.Setup(t => t.BuildProgressInformation).Returns(new ThoughtWorks.CruiseControl.Core.Util.BuildProgressInformation("", ""));
            mock.Setup(t => t.AddTaskResult(It.IsAny<string>()));
            mock.Setup(t => t.BaseFromWorkingDirectory(It.IsAny<string>())).Returns(() => @"D:\TEST\RSYNC\SRC");
            mock.Setup(t => t.IntegrationProperties).Returns(() => new Dictionary<string, string>());

            var task = new TqRsyncTask();
            task.WorkingDirectory = @"D:\TEST\RSYNC\SRC";
            task.Src = "./";
            task.Dest = "testuser@192.168.3.206::RSYNC_TEST/DESC";
            task.Password = "1234";
            task.Options = "-avrzP";

            task.Run(mock.Object);

            Assert.AreEqual(true, task.WasSuccessful);
        }
    }
}