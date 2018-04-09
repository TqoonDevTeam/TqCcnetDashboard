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
        //[Ignore("로컬전용")]
        public void TqGitCITask()
        {
            var result = new IntegrationResult("test", @"D:\Test\CI", @"D:\Test\CI", new IntegrationRequest(BuildCondition.ForceBuild, "", ""), new IntegrationSummary(IntegrationStatus.Success, "", "", DateTime.Now));

            TqGitCITask task = new TqGitCITask();
            task.GitRepository = "https://a3c9a3a71d88ead6dddf9562e5d75b90d9bfc7e5@github.com/TqoonDevTeam/TqoonLibraries.git";

            task.Branch = "group1";
            task.StartBranch = "dev";

            task.Run(result);
        }
    }
}