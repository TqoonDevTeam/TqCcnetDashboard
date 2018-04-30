using Exortech.NetReflector;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.Utils;

namespace TqLib.ccnet.Core.Sourcecontrol
{
    [ReflectorType("TqGitCI", Description = "TqGitCI")]
    public class TqGitCI : SourceControlBase
    {
        [ReflectorProperty("repository")]
        public string GitRepository { get; set; }

        [ReflectorProperty("autoGetSource", Required = false)]
        public bool AutoGetSource { get; set; }

        [ReflectorProperty("branch", Required = true)]
        public string Branch { get; set; }

        [ReflectorProperty("startBranch")]
        public string StartBranch { get; set; }

        [ReflectorProperty("gitUserId")]
        public string GitUserId { get; set; } = string.Empty;

        [ReflectorProperty("gitUserPassword")]
        public string GitUserPassword { get; set; } = string.Empty;

        [ReflectorProperty("tagOnSuccess", Required = false)]
        public bool TagOnSuccess { get; set; }

        private string projectName, gitDirectory, gitUrl;
        private bool isFirstRun;
        private GitUtil git;

        public override Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
        {
            Dictionary<string, string> dictionary = NameValuePair.ToDictionary(from.SourceControlData);
            IList<Modification> changeList = new List<Modification>();

            InitProcessData(to);
            InitGitRepository(to);

            git.Reset(to, Branch);
            git.Reset(to, StartBranch);

            git.FetchOrigin(to);

            // StartBranch
            git.Checkout(to, StartBranch);
            var diffOriginStartBranch = git.GetDiffList(to, $"origin/{StartBranch}");
            var startBranchPullResult = git.Pull(to);
            foreach (var item in startBranchPullResult.Item2)
            {
                changeList.Add(item);
            }

            // Branch
            git.Checkout(to, Branch);
            var diffOriginBranch = git.GetDiffList(to, $"origin/{Branch}");
            var branchPullResult = git.Pull(to);
            foreach (var item in branchPullResult.Item2)
            {
                changeList.Add(item);
            }

            if (changeList.Count == 0 && from.LastBuildStatus != IntegrationStatus.Success)
            {
                foreach (var change in from.Modifications)
                {
                    changeList.Add(change);
                }
            }

            if (isFirstRun || from.LastBuildStatus != IntegrationStatus.Success)
            {
                if (changeList.Count == 0)
                {
                    changeList.Add(new Modification
                    {
                        ChangeNumber = "Initialize",
                        Comment = "Initialize",
                        FileName = "",
                        FolderName = "",
                        ModifiedTime = DateTime.MinValue,
                        EmailAddress = "Initialize",
                        UserName = "Initialize",
                        Type = "Unmodified"
                    });
                }
            }

            to.SourceControlData.Clear();
            NameValuePair.Copy(dictionary, to.SourceControlData);
            to.SetSourceData("$TqGitCI_repository", gitUrl);
            to.SetSourceData("$TqGitCI_gitDirectory", gitDirectory);
            to.SetSourceData("$TqGitCI_branch", Branch);
            to.SetSourceData("$TqGitCI_startBranch", StartBranch);
            to.SetSourceData("$TqGitCI_projectName", projectName);
            to.SetSourceData("$TqGitCI_hasDiffOrigin1", (diffOriginBranch.Count > 0).ToString());
            to.SetSourceData("$TqGitCI_pullResult1", branchPullResult.Item1.Status.ToString());
            to.SetSourceData("$TqGitCI_lastCommitter1", branchPullResult.Item2.LastOrDefault()?.UserName ?? string.Empty);
            to.SetSourceData("$TqGitCI_hasDiffOrigin2", (diffOriginStartBranch.Count > 0).ToString());
            to.SetSourceData("$TqGitCI_pullResult2", startBranchPullResult.Item1.Status.ToString());
            to.SetSourceData("$TqGitCI_lastCommitter2", startBranchPullResult.Item2.LastOrDefault()?.UserName ?? string.Empty);

            to.SetParameters("$TqGitCI_gitUserId", GitUserId);
            to.SetParameters("$TqGitCI_gitUserPassword", GitUserPassword);

            return changeList.ToArray();
        }

        public override void GetSource(IIntegrationResult result)
        {
            if (!AutoGetSource)
            {
                return;
            }
            InitProcessData(result);
            InitGitRepository(result);
        }

        private void InitProcessData(IIntegrationResult result)
        {
            result.AddMessage("#InitProcessData");
            projectName = GetGitRepositoryName();

            gitDirectory = result.BaseFromWorkingDirectory(projectName);

            var builder = new UriBuilder(GitRepository);
            builder.UserName = string.Empty;
            builder.Password = string.Empty;
            gitUrl = builder.Uri.ToString();

            git = new GitUtil(projectName, gitDirectory, gitUrl, GitUserId, GitUserPassword);

            isFirstRun = false;
        }

        private void InitGitRepository(IIntegrationResult result)
        {
            result.AddMessage("#InitGitRepository");
            if (Directory.Exists(gitDirectory))
            {
                if (!Repository.IsValid(gitDirectory))
                {
                    Directory.Delete(gitDirectory, true);
                    git.GitClone(result, Branch);
                    isFirstRun = true;
                }
            }
            else
            {
                git.GitClone(result, Branch);
                isFirstRun = true;
            }
        }

        private string GetGitRepositoryName()
        {
            return Path.GetFileNameWithoutExtension(GitRepository.Substring(GitRepository.LastIndexOf('/')));
        }

        public override void Initialize(IProject project)
        {
        }

        public override void LabelSourceControl(IIntegrationResult result)
        {
            if (TagOnSuccess && result.Succeeded)
            {
                // Branch
                git.Checkout(result, Branch);
                // Push
                git.Push(result, Branch);
            }
        }

        public override void Purge(IProject project)
        {
        }
    }
}