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

        [ReflectorProperty("UserId")]
        public string UserId { get; set; } = string.Empty;

        [ReflectorProperty("UserPassword")]
        public string UserPassword { get; set; } = string.Empty;

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

            result.AddMessage("git clone");
            GitClone();

            result.AddMessage($"git checkout {Branch}");
            CheckoutOrSwitch();

            result.AddMessage("git pull");
            var pullResult = Pull();
            result.AddMessage("status: " + pullResult.Status.ToString());

            result.AddMessage($"git fetch origin/{StartBranch}");
            FetchStartBranch();

            result.AddMessage($"git diff origin/{StartBranch}");
            var diffList = GetDiffList();

            if (diffList.Count > 0)
            {
                result.AddMessage($"git merge origin/{StartBranch}");
                var mergeResult = Merge();
                if (mergeResult.Status == MergeStatus.Conflicts)
                {
                    var conflictsList = GetConflictsList();
                    result.AddMessage($"[TqGitCTask] Conflicts {Branch} <- {StartBranch}");
                    result.AddMessage("Conflicts Count: " + conflictsList.Count);
                    result.AddMessage("git reset");
                    Reset();
                    return false;
                }
                else
                {
                    if (CheckOnly)
                    {
                        result.AddMessage("git reset");
                        Reset();
                    }
                    else
                    {
                        result.AddMessage($"git push");
                        bool pushSuccess = false;
                        try
                        {
                            Push();
                            pushSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            Reset();
                            throw ex;
                        }
                        finally
                        {
                            result.AddMessage($"[TqGitCTask] merge {Branch} <- {StartBranch} {(pushSuccess ? "Success" : "Push Exception")}");
                        }
                    }
                }
            }
            return true;
        }

        private IList<Conflict> GetConflictsList()
        {
            using (var repo = GetRepository())
            {
                var conflictList = repo.Index.Conflicts.ToList();
                return conflictList;
            }
        }

        private void Reset()
        {
            using (var repo = GetRepository())
            {
                Branch origin = repo.Branches[$"origin/{Branch}"];
                repo.Reset(ResetMode.Hard, origin.Tip);
            }
        }

        private string GitClone()
        {
            if (!Directory.Exists(gitDirectory))
            {
                return Repository.Clone(gitUrl, gitDirectory, GetCloneOptions());
            }
            else
            {
                FetchOrigin();
            }
            return string.Empty;
        }

        private Branch CheckoutOrSwitch()
        {
            using (var repo = GetRepository())
            {
                Branch branch = repo.Branches[Branch];
                if (branch == null)
                {
                    var remoteBranch = repo.Branches["origin/" + Branch];
                    if (remoteBranch.IsRemote)
                    {
                        var newBranch = repo.CreateBranch(Branch, remoteBranch.Tip);
                        repo.Branches.Update(newBranch, t => t.TrackedBranch = remoteBranch.CanonicalName);
                        branch = repo.Branches[Branch];
                    }
                }
                return Commands.Checkout(repo, branch);
            }
        }

        private MergeResult Pull()
        {
            using (var repo = GetRepository())
            {
                return Commands.Pull(repo, GetSignature(), GetPullOptions());
            }
        }

        private MergeResult Merge()
        {
            using (var repo = GetRepository())
            {
                return repo.Merge(repo.Branches["origin/" + StartBranch], GetSignature());
            }
        }

        private void Push()
        {
            using (var repo = GetRepository())
            {
                repo.Network.Push(repo.Branches["dev"], GetPushOptions());
            }
        }

        private IList<TreeEntryChanges> GetDiffList()
        {
            using (var repo = GetRepository())
            {
                return repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, repo.Branches["origin/" + StartBranch].Tip.Tree).ToList();
            }
        }

        private void FetchOrigin()
        {
            using (var repo = GetRepository())
            {
                foreach (Remote remote in repo.Network.Remotes)
                {
                    string logMessage = "";
                    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, GetFetchOptions(), logMessage);
                }
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

        private CloneOptions GetCloneOptions()
        {
            CloneOptions co = new CloneOptions();
            co.CredentialsProvider = GetCredentialsHandler();
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
            return (_url, _user, _cred) => new UsernamePasswordCredentials { Username = UserId, Password = UserPassword };
        }

        private Signature GetSignature()
        {
            return new Signature(UserName, UserEmail, new DateTimeOffset(DateTime.Now));
        }
    }
}