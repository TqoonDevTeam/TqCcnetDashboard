using Exortech.NetReflector;
using LibGit2Sharp;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using TqLib.Utils;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqGitCIMerge", Description = "TqGitCIMerge")]
    public class TqGitCIMergeTask : TaskBase
    {
        private GitUtil git;

        protected override bool Execute(IIntegrationResult result)
        {
            string repository = result.GetSourceData("$TqGitCI_repository");
            string gitDirectory = result.GetSourceData("$TqGitCI_gitDirectory");
            string branch = result.GetSourceData("$TqGitCI_branch");
            string startBranch = result.GetSourceData("$TqGitCI_startBranch");
            string projectName = result.GetSourceData("$TqGitCI_projectName");
            string gitUserId = result.GetParameters("$TqGitCI_gitUserId");
            string gitUserPassword = result.GetParameters("$TqGitCI_gitUserPassword");

            git = new GitUtil(projectName, gitDirectory, repository, gitUserId, gitUserPassword);

            // Branch
            git.Checkout(result, branch);
            var diffBranchAndStartBranch = git.GetDiffList(result, startBranch);
            if (diffBranchAndStartBranch.Count > 0)
            {
                var mergeResult = git.Merge(result, startBranch);
                result.SetSourceData("$TqGitCI_mergeResult", mergeResult.Status.ToString());
                if (mergeResult.Status == MergeStatus.Conflicts)
                {
                    var conflictsList = git.GetConflictsList(result);
                    result.SetSourceData("$TqGitCI_mergeExceptionMessage", $"{projectName} Conflicts({conflictsList.Count}) {branch} <- {startBranch}");
                    return false;
                }
            }
            return true;
        }
    }
}