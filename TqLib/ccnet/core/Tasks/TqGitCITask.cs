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
            repoName = GetGitRepositoryName();
            gitDirectory = result.BaseFromWorkingDirectory(repoName);

            var builder = new UriBuilder(GitRepository);
            builder.UserName = string.Empty;
            builder.Password = string.Empty;
            gitUrl = builder.Uri.ToString();

            GitClone(result, Branch);
            Fetch(result);
            Pull(result);

            CheckoutOrSwitch(result, StartBranch);
            Pull(result);

            CheckoutOrSwitch(result, Branch);
            var diffList = GetDiffList(result, StartBranch);
            if (diffList.Count > 0)
            {
                var mergeResult = Merge(result, StartBranch);
                if (mergeResult.Status == MergeStatus.Conflicts)
                {
                    var conflictsList = GetConflictsList();
                    result.AddMessage($"[TqGitCTask] {repoName} Conflicts {Branch} <- {StartBranch}, ConflictsCount {conflictsList.Count}");
                    Reset(result);
                    return false;
                }
                else
                {
                    result.AddMessage($"CheckOnly {CheckOnly}");
                    if (CheckOnly)
                    {
                        Reset(result);
                    }
                    else
                    {
                        if (!(mergeResult.Commit == null || mergeResult.Status == MergeStatus.UpToDate))
                        {
                            bool pushSuccess = false;
                            try
                            {
                                Push(result);
                                pushSuccess = true;
                            }
                            catch (Exception ex)
                            {
                                Reset(result);
                                throw ex;
                            }
                            finally
                            {
                                result.AddMessage($"[TqGitCTask] {repoName} merge {Branch} <- {StartBranch} {(pushSuccess ? "Success" : "Push Exception")}");
                            }
                        }
                    }
                }
            }
            return true;
        }

        private string GitClone(IIntegrationResult result, string branch)
        {
            result.AddMessage($"git clone {gitUrl} {gitDirectory}");
            if (!Directory.Exists(gitDirectory))
            {
                return Repository.Clone(gitUrl, gitDirectory, GetCloneOptions(branch));
            }
            return string.Empty;
        }

        private void Fetch(IIntegrationResult result)
        {
            result.AddMessage("git fetch origin");
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

        private Branch CheckoutOrSwitch(IIntegrationResult result, string branchName)
        {
            result.AddMessage($"git checkout {branchName}");
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
            result.AddMessage("git pull");
            MergeResult mergeResult;
            using (var repo = GetRepository())
            {
                mergeResult = Commands.Pull(repo, GetSignature(), GetPullOptions());
                result.AddMessage($"pull result : {mergeResult.Status}");
            }
            return mergeResult;
        }

        private IList<TreeEntryChanges> GetDiffList(IIntegrationResult result, string target)
        {
            result.AddMessage($"git diff {target}");
            IList<TreeEntryChanges> diffList;
            using (var repo = GetRepository())
            {
                diffList = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, repo.Branches[target].Tip.Tree).ToList();
            }
            result.AddMessage($"diff list : {Environment.NewLine}{string.Join(Environment.NewLine, diffList.Select(t => t.Path))}");
            return diffList;
        }

        private MergeResult Merge(IIntegrationResult result, string branch)
        {
            result.AddMessage($"git merge {branch}");
            using (var repo = GetRepository())
            {
                return repo.Merge(repo.Branches[branch], GetSignature(), GetMergeOptions());
            }
        }

        private void Reset(IIntegrationResult result)
        {
            result.AddMessage("git reset");
            using (var repo = GetRepository())
            {
                Branch origin = repo.Branches[$"origin/{Branch}"];
                repo.Reset(ResetMode.Hard, origin.Tip);
            }
        }

        private void Push(IIntegrationResult result)
        {
            result.AddMessage("git push");
            using (var repo = GetRepository())
            {
                repo.Network.Push(repo.Branches[Branch], GetPushOptions());
            }
        }

        private IList<Conflict> GetConflictsList()
        {
            using (var repo = GetRepository())
            {
                var conflictList = repo.Index.Conflicts.ToList();
                return conflictList;
            }
        }

        private void FetchStartBranch()
        {
            using (var repo = GetRepository())
            {
                foreach (var remote in repo.Network.Remotes)
                {
                    string logMessage = "";
                    var refspecs = remote.FetchRefSpecs.Select(t => t.Specification.Replace("*", StartBranch));
                    Commands.Fetch(repo, remote.Name, refspecs, GetFetchOptions(), logMessage);
                }
            }
        }

        private string GetGitRepositoryName()
        {
            return Path.GetFileNameWithoutExtension(GitRepository.Substring(GitRepository.LastIndexOf('/')));
        }

        private Repository GetRepository()
        {
            return new Repository(gitDirectory);
        }

        private CloneOptions GetCloneOptions(string branch)
        {
            CloneOptions co = new CloneOptions();
            co.CredentialsProvider = GetCredentialsHandler();
            co.BranchName = branch;
            return co;
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