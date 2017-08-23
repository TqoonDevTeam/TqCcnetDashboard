using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TqLib.ccnet.Local.Helper
{
    public class TqCcnetDashboardUpdator
    {
        public readonly object lock_msg = new object();
        public bool IsBysy = false;
        public string Owner { get; set; } = "TqoonDevTeam";
        public string Name { get; set; } = "TqCcnetDashboard";
        public string DownloadFolder { get; set; }
        public string DashboardFolder { get; set; }
        public string PluginFolder { get; set; }
        public string ServiceFolder { get; set; }
        public List<TqCcnetDashboardUpdatorEventArgs> AllMessage { get; private set; } = new List<TqCcnetDashboardUpdatorEventArgs>();
        public TqCcnetDashboardUpdatorEventArgs CurrentMessage { get; private set; } = null;

        public event EventHandler<List<TqCcnetDashboardUpdatorEventArgs>> ProcessChanged;

        public void Update()
        {
            if (!IsBysy)
            {
                AllMessage.Clear();
                IsBysy = true;
                CheckDownloadFolder();
                Download();
                ExtractZip();
                StopService();
                //Deploy();
                StartService();
                IsBysy = false;
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { FileName = "", Desc = "Complete", Msg = "Complete", ProgressPercentage = 100 });
            }
        }

        public void UpdateExternalPlugin(string externalPluginPath)
        {
            if (!IsBysy)
            {
                AllMessage.Clear();
                IsBysy = true;
                ExtractZip();
                StopService();
                // DeployExternalPlugin();
                StartService();
                IsBysy = false;
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { FileName = "", Desc = "Complete", Msg = "Complete", ProgressPercentage = 100 });
            }
        }

        public string GetRemoteVersion()
        {
            return new TqDashboardRemoteVersionChecker().GetRemoteVersion();
        }

        public void CheckDownloadFolder()
        {
            OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "다운로드폴더확인", Msg = "폴더를 비웁니다.", ProgressPercentage = 100 });
            if (Directory.Exists(DownloadFolder)) Directory.Delete(DownloadFolder, true);
            Directory.CreateDirectory(DownloadFolder);
        }

        private void Download()
        {
            var version = GetRemoteVersion();

            var downloaders = new AssetDownloader[] {
                new AssetDownloader($"https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/download/{version}/plugins.zip", "plugins.zip") { DownloadFolder = DownloadFolder },
                new AssetDownloader($"https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/download/{version}/web.zip", "web.zip") { DownloadFolder = DownloadFolder },
            };

            foreach (var downloader in downloaders)
            {
                downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                downloader.Download();
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, AssetDownloader.AssertDownloadEventArgs e)
        {
            var evt = new TqCcnetDashboardUpdatorEventArgs();
            evt.FileName = Path.GetFileName(e.FilePath);
            evt.Desc = $"다운로드 - {evt.FileName}";
            evt.ProgressPercentage = e.ProgressPercentage;
            OnProcessChanged(evt);
        }

        private void ExtractZip()
        {
            string fileName;
            foreach (var zip in Directory.GetFiles(DownloadFolder, "*.zip"))
            {
                fileName = Path.GetFileName(zip);
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = $"압축해제 - {fileName}", FileName = fileName, ProgressPercentage = 0 });
                ZipFile.ExtractToDirectory(zip, Path.Combine(Path.GetDirectoryName(zip), Path.GetFileNameWithoutExtension(zip)));
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = $"압축해제 - {fileName}", FileName = fileName, ProgressPercentage = 100 });
            }
        }

        //private bool Deploy()
        //{
        //    var downloadPluginsFolder = Path.Combine(DownloadFolder, "plugins");
        //    if (Directory.Exists(downloadPluginsFolder))
        //    {
        //        OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "기본플러그인설치", FileName = "plugins", ProgressPercentage = 0 });
        //        var pluginUpdator = new CcnetPluginInstaller(downloadPluginsFolder, PluginFolder, ServiceFolder);
        //        pluginUpdator.Install();
        //        OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "기본플러그인설치", FileName = "plugins", ProgressPercentage = 100 });
        //    }

        //    var downloadWebFolder = Path.Combine(DownloadFolder, @"web");
        //    if (Directory.Exists(downloadWebFolder))
        //    {
        //        OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "대시보드설치", FileName = "web", ProgressPercentage = 0 });
        //        var dashboardFolder = DashboardFolder.EndsWith(@"\") ? DashboardFolder.Substring(0, DashboardFolder.Length - 1) : DashboardFolder;
        //        var robo = new ExecutableUtil()
        //        {
        //            Executable = "robocopy.exe",
        //            SuccessCodes = new[] { 0, 1 },
        //            Args = $"\"{downloadWebFolder}\" \"{dashboardFolder}\" /E /PURGE /XA:H /XD Config"
        //        };
        //        var result = robo.Run();
        //        if (result.WasSuccess)
        //        {
        //            OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "대시보드설치", FileName = "web", ProgressPercentage = 100 });
        //        }
        //        else
        //        {
        //            OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "대시보드설치 - 오류", FileName = "web", Msg = result.Error + "\n" + result.Output, ProgressPercentage = 0 });
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        //private bool DeployExternalPlugin()
        //{
        //    foreach (var extractZipFolder in Directory.GetDirectories(DownloadFolder))
        //    {
        //        if (Directory.Exists(extractZipFolder))
        //        {
        //            OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "기본플러그인설치", FileName = "plugins", ProgressPercentage = 0 });
        //            var pluginUpdator = new CcnetPluginInstaller(extractZipFolder, PluginFolder, ServiceFolder);
        //            pluginUpdator.DisAllowFIleNames = new[] { "ThoughtWorks.CruiseControl.Core.dll", "ThoughtWorks.CruiseControl.Remote" };
        //            pluginUpdator.Install();
        //            OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "기본플러그인설치", FileName = "plugins", ProgressPercentage = 100 });
        //        }
        //    }
        //    return true;
        //}

        private void StopService()
        {
            using (var ccnet = new CCNET())
            {
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "ServiceStop - CCNET", FileName = "", Msg = "", ProgressPercentage = 0 });
                ccnet.StopService();
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "ServiceStop - CCNET", FileName = "", Msg = "", ProgressPercentage = 100 });
            }
        }

        private void StartService()
        {
            using (var ccnet = new CCNET())
            {
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "ServiceStart - CCNET", FileName = "", Msg = "", ProgressPercentage = 0 });
                ccnet.StartService();
                OnProcessChanged(new TqCcnetDashboardUpdatorEventArgs { Desc = "ServiceStart - CCNET", FileName = "", Msg = "", ProgressPercentage = 100 });
            }
        }

        private void OnProcessChanged(TqCcnetDashboardUpdatorEventArgs e)
        {
            lock (lock_msg)
            {
                CurrentMessage = e;
                var find = AllMessage.FirstOrDefault(t => t.Desc == e.Desc);
                if (find == null)
                {
                    AllMessage.Add(e);
                }
                else
                {
                    find.Merge(e);
                }
                ProcessChanged?.Invoke(this, AllMessage);
            }
        }

        public class AssetDownloader
        {
            private string _Token;

            private string _FileName, _Url;
            private string _downloadPath { get; set; }
            public string DownloadFolder { get; set; }

            public event EventHandler<AssertDownloadEventArgs> DownloadProgressChanged;

            public AssetDownloader(string url, string fileName)
            {
                _FileName = fileName;
                _Url = url;
            }

            public string Download()
            {
                _downloadPath = Path.Combine(DownloadFolder, _FileName);
                Task.Run(async () =>
                {
                    await DownloadStart();
                }).Wait();
                return _downloadPath;
            }

            private Task DownloadStart()
            {
                var client = new WebClient();
                client.Headers[HttpRequestHeader.UserAgent] = "Awesome-Octocat-App-TqoonDevTeam";
                client.Headers[HttpRequestHeader.Accept] = "application/octet-stream";
                if (!string.IsNullOrEmpty(_Token))
                {
                    client.Headers[HttpRequestHeader.Authorization] = "token " + _Token;
                }
                client.DownloadProgressChanged += (s, e) =>
                {
                    if (DownloadProgressChanged != null)
                    {
                        DownloadProgressChanged.Invoke(this, new AssertDownloadEventArgs { FilePath = _downloadPath, ProgressPercentage = e.ProgressPercentage });
                    }
                    client.Dispose();
                };
                return client.DownloadFileTaskAsync(_Url, _downloadPath);
            }

            public class AssertDownloadEventArgs : EventArgs
            {
                public string FilePath { get; set; }
                public int ProgressPercentage { get; set; }
            }
        }

        public class TqCcnetDashboardUpdatorEventArgs
        {
            public string Desc { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public int ProgressPercentage { get; set; }
            public string Msg { get; set; } = string.Empty;

            public void Merge(TqCcnetDashboardUpdatorEventArgs item)
            {
                FileName = item.FileName;
                ProgressPercentage = item.ProgressPercentage;
                Msg = item.Msg;
            }
        }
    }
}