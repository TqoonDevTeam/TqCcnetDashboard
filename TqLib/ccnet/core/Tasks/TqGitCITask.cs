using Exortech.NetReflector;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqGitCI", Description = "TqGitCI")]
    public class TqGitCITask : TaskBase
    {
        [ReflectorProperty("GitRepository")]
        public string GitRepository { get; set; }

        [ReflectorProperty("Branch")]
        public string Branch { get; set; }

        [ReflectorProperty("StartBranch")]
        public string StartBranch { get; set; }

        [ReflectorProperty("CheckOnly")]
        public bool CheckOnly { get; set; } = false;

        [ReflectorProperty("GitUserId")]
        public string GitUserId { get; set; } = string.Empty;

        [ReflectorProperty("GitUserPassword")]
        public string GitUserPassword { get; set; } = string.Empty;

        [ReflectorProperty("UserName", Required = false)]
        public string UserName { get; set; } = "TqGitCITask";

        [ReflectorProperty("UserEmail", Required = false)]
        public string UserEmail { get; set; } = "TqGitCITask";
        private string repoName, gitDirectory, gitUrl;

        protected override bool Execute(IIntegrationResult result)
        {
            InitProcessData(result);
            InitGitRepository(result);

            FetchOrigin(result);

            var diffOriginBranch = GetDiffList(result, $"origin/{Branch}");
            var branchPullResult = Pull(result);

            Checkout(result, StartBranch);
            var diffOriginStartBranch = GetDiffList(result, $"origin/{StartBranch}");
            var startBranchPullResult = Pull(result);

            Checkout(result, Branch);
            var diffBranchAndStartBranch = GetDiffList(result, StartBranch);
            if (diffBranchAndStartBranch.Count > 0)
            {
                var mergeResult = Merge(result, StartBranch);
                if (mergeResult.Status == MergeStatus.Conflicts)
                {
                    var conflictsList = GetConflictsList(result);
                    result.AddMessage($"[TqGitCTask] {repoName} {Branch} Conflicts {Branch} <- {StartBranch}, ConflictsCount {conflictsList.Count}");
                    Reset(result);
                    return false;
                }
                else
                {
                    if (CheckOnly)
                    {
                        result.AddMessage($"#CheckOnly True > Reset");
                        Reset(result);
                    }
                    else
                    {
                        if (!(mergeResult.Commit == null || mergeResult.Status == MergeStatus.UpToDate))
                        {
                            try
                            {
                                Push(result);
                                result.AddMessage($"push success");
                            }
                            catch (Exception ex)
                            {
                                result.AddMessage($"[TqGitCITask] {repoName} {Branch} Push Exception");
                                Reset(result);
                                throw ex;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void InitProcessData(IIntegrationResult result)
        {
            result.AddMessage("#InitProcessData");
            repoName = GetGitRepositoryName();

            gitDirectory = result.BaseFromWorkingDirectory(repoName);

            var builder = new UriBuilder(GitRepository);
            builder.UserName = string.Empty;
            builder.Password = string.Empty;
            gitUrl = builder.Uri.ToString();
        }
        private void InitGitRepository(IIntegrationResult result)
        {
            result.AddMessage("#InitGitRepository");
            if (Directory.Exists(gitDirectory))
            {
                if (!Repository.IsValid(gitDirectory))
                {
                    Directory.Delete(gitDirectory, true);
                    GitClone(result, Branch);
                }
            }
            else
            {
                GitClone(result, Branch);
            }
        }
        private string GitClone(IIntegrationResult result, string branch)
        {
            result.AddMessage($"#GitClone {branch}");
            CloneOptions co = new CloneOptions();
            co.CredentialsProvider = GetCredentialsHandler();
            co.BranchName = branch;
            return Repository.Clone(gitUrl, gitDirectory, co);
        }
        private void FetchOrigin(IIntegrationResult result)
        {
            result.AddMessage("#FetchOrigin");
            using (var repo = GetRepository())
            {
                string logMessage = "";
                foreach (Remote remote in repo.Network.Remotes)
                {
                    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, GetFetchOptions(), logMessage);
                    result.AddMessage(logMessage);
                }
            }
        }
        private Branch Checkout(IIntegrationResult result, string branchName)
        {
            result.AddMessage($"#Checkout {branchName}");
            using (var repo = GetRepository())
            {
                Branch branch = repo.Branches[branchName];
                if (branch == null)
                {
                    branch = repo.CreateBranch(branchName, $"origin/{branchName}");
                    var remoteBranch = repo.Branches[$"origin/{branchName}"];
                    repo.Branches.Update(branch
                        , b => b.UpstreamBranch = remoteBranch.UpstreamBranchCanonicalName
                        , b => b.TrackedBranch = remoteBranch.CanonicalName
                        );
                    branch = repo.Branches[branchName];
                }
                return Commands.Checkout(repo, branch);
            }
        }
        private MergeResult Pull(IIntegrationResult result)
        {
            result.AddMessage("#Pull");
            MergeResult mergeResult;
            using (var repo = GetRepository())
            {
                mergeResult = Commands.Pull(repo, GetSignature(), GetPullOptions());
                result.AddMessage($"pullresult : {mergeResult.Status}");
            }
            return mergeResult;
        }
        private IList<TreeEntryChanges> GetDiffList(IIntegrationResult result, string target)
        {
            result.AddMessage($"#GetDiffList {target}");
            IList<TreeEntryChanges> diffList;
            using (var repo = GetRepository())
            {
                diffList = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, repo.Branches[target].Tip.Tree).ToList();
            }
            if (diffList.Count > 0)
            {
                result.AddMessage(string.Join(Environment.NewLine, diffList.Select(t => t.Path)));
            }
            return diffList;
        }
        private MergeResult Merge(IIntegrationResult result, string branch)
        {
            result.AddMessage($"#Merge {branch}");
            using (var repo = GetRepository())
            {
                return repo.Merge(repo.Branches[branch], GetSignature(), GetMergeOptions());
            }
        }
        private void Reset(IIntegrationResult result)
        {
            result.AddMessage("#Reset");
            using (var repo = GetRepository())
            {
                Branch origin = repo.Branches[$"origin/{Branch}"];
                repo.Reset(ResetMode.Hard, origin.Tip);
            }
        }
        private void Push(IIntegrationResult result)
        {
            result.AddMessage("#Push");
            using (var repo = GetRepository())
            {
                repo.Network.Push(repo.Branches[Branch], GetPushOptions());
            }
        }
        private IList<Conflict> GetConflictsList(IIntegrationResult result)
        {
            result.AddMessage("#GetConflictsList");

            IList<Conflict> conflictList;
            using (var repo = GetRepository())
            {
                conflictList = repo.Index.Conflicts.ToList();
            }
            if (conflictList.Count > 0)
            {
                result.AddMessage(string.Join(Environment.NewLine, conflictList.Select(t => t.Ours.Path)));
            }
            return conflictList;
        }
        private string GetGitRepositoryName()
        {
            return Path.GetFileNameWithoutExtension(GitRepository.Substring(GitRepository.LastIndexOf('/')));
        }
        private Repository GetRepository()
        {
            return new Repository(gitDirectory);
        }
        private FetchOptions GetFetchOptions()
        {
            FetchOptions fo = new FetchOptions();
            fo.CredentialsProvider = GetCredentialsHandler();
            return fo;
        }
        private PullOptions GetPullOptions()
        {
            PullOptions po = new PullOptions();
            po.FetchOptions = GetFetchOptions();
            po.MergeOptions = GetMergeOptions();
            return po;
        }
        private MergeOptions GetMergeOptions()
        {
            return new MergeOptions()
            {
            };
        }
        private PushOptions GetPushOptions()
        {
            var po = new PushOptions();
            po.CredentialsProvider = GetCredentialsHandler();
            return po;
        }
        private CredentialsHandler GetCredentialsHandler()
        {
            return (_url, _user, _cred) => new UsernamePasswordCredentials { Username = GitUserId, Password = GitUserPassword };
        }
        private Signature GetSignature()
        {
            return new Signature(UserName, UserEmail, new DateTimeOffset(DateTime.Now));
        }
    }
}