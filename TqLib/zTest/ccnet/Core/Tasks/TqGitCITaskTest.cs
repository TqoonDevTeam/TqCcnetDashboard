using NUnit.Framework;
using System;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.ccnet.Core.Tasks;

namespace TqLib.zTest.ccnet.Core.Tasks
{
    [TestFixture(Category = "ccnet.core.Tasks")]
    public class TqGitCITaskTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void TqGitCITask()
        {
            var result = new IntegrationResult("test", @"D:\Test", @"D:\Test", new IntegrationRequest(BuildCondition.ForceBuild, "", ""), new IntegrationSummary(IntegrationStatus.Success, "", "", DateTime.Now));

            TqGitCITask task = new TqGitCITask();
            task.GitRepository = "https://github.com/wisdomcdh/Test.git";

            task.Branch = "dev";
            task.StartBranch = "master";

            task.Run(result);
        }
    }
}