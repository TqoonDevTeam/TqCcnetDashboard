using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using TqLib.ccnet.Core;

namespace TqLib.Utils
{
    public class GitUtil
    {
        public string ProjectName { get; private set; }
        public string GitDirectory { get; private set; }
        public string SignatureName { get; private set; } = "TqGitCI";
        public string SignatureEmail { get; private set; } = "TqGitCI";
        public string GitRepository { get; private set; }
        private string GitUserId, GitUserPassword;

        public GitUtil(string projectName, string gitDirectory, string gitRepository, string gitUserId, string gitUserPassword)
        {
            ProjectName = projectName;
            GitDirectory = gitDirectory;
            GitUserId = gitUserId;
            GitUserPassword = gitUserPassword;
            GitRepository = gitRepository;
        }

        public string GitClone(IIntegrationResult result, string branch)
        {
            result.AddMessage($"#GitClone {branch}");
            CloneOptions co = new CloneOptions();
            co.CredentialsProvider = GetCredentialsHandler();
            co.BranchName = branch;
            return Repository.Clone(GitRepository, GitDirectory, co);
        }

        public void FetchOrigin(IIntegrationResult result)
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

        public Branch Checkout(IIntegrationResult result, string branchName)
        {
            result.AddMessage($"#Checkout {branchName}");
            using (var repo = GetRepository())
            {
                if (repo.Head.FriendlyName == branchName) return repo.Branches[branchName];

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

        public MergeResult Pull(IIntegrationResult result)
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

        public IList<TreeEntryChanges> GetDiffList(IIntegrationResult result, string target)
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

        public MergeResult Merge(IIntegrationResult result, string branch)
        {
            result.AddMessage($"#Merge {branch}");
            using (var repo = GetRepository())
            {
                return repo.Merge(repo.Branches[branch], GetSignature(), GetMergeOptions());
            }
        }

        public void Reset(IIntegrationResult result, string branch)
        {
            result.AddMessage($"#Reset {branch}");
            Checkout(result, branch);
            using (var repo = GetRepository())
            {
                Branch origin = repo.Branches[$"origin/{branch}"];
                repo.Reset(ResetMode.Hard, origin.Tip);
            }
        }

        public void Push(IIntegrationResult result, string branch)
        {
            result.AddMessage("#Push");
            using (var repo = GetRepository())
            {
                repo.Network.Push(repo.Branches[branch], GetPushOptions());
            }
        }

        public IList<Conflict> GetConflictsList(IIntegrationResult result)
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

        public IList<Modification> GetChangeList(MergeResult pullResult)
        {
            IList<Modification> changeList = new List<Modification>();
            using (var repo = GetRepository())
            {
                if (pullResult.Commit != null)
                {
                    foreach (var parent in pullResult.Commit.Parents)
                    {
                        foreach (var change in repo.Diff.Compare<TreeChanges>(parent.Tree, pullResult.Commit.Tree))
                        {
                            changeList.Add(new Modification
                            {
                                ChangeNumber = parent.Sha,
                                Comment = parent.MessageShort,
                                FileName = Path.GetFileName(change.Path),
                                FolderName = Path.GetDirectoryName(change.Path),
                                ModifiedTime = parent.Committer.When.DateTime,
                                EmailAddress = parent.Committer.Email,
                                UserName = parent.Author.Name,
                                Type = change.Status.ToString()
                            });
                        }
                    }
                }
            }
            return changeList;
        }

        private Repository GetRepository()
        {
            return new Repository(GitDirectory);
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
            return new Signature(SignatureName, SignatureEmail, new DateTimeOffset(DateTime.Now));
        }
    }
}