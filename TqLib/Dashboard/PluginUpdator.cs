using log4net;
using System.IO;
using System.IO.Compression;
using TqLib.ccnet.Local;
using TqLib.Dashboard.CcnetPluginInstall;
using TqLib.Utils;

namespace TqLib.Dashboard
{
    public class PluginUpdator
    {
        public ILog Logger { get; set; }
        public ILog SystemLogger { get; set; }
        public string DownloadFolder { get; set; }
        public string DownloadUrl { get; set; } = "https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/download/1.0.0.0/plugins.zip";
        public string PluginDirectory { get; set; }
        public string ServiceDirecotry { get; set; }

        public void Update()
        {
            CheckDownloadFolder();
            Download();
            UnZip();
            StopService();
            Deploy();
            StartService();
        }

        private void CheckDownloadFolder()
        {
            Logger?.Info("다운로드 폴더 확인");
            if (Directory.Exists(DownloadFolder)) Directory.Delete(DownloadFolder, true);
            Directory.CreateDirectory(DownloadFolder);
        }

        private void Download()
        {
            if (DownloadUrl.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
            {
                var downloader = new AssetDownloader(DownloadUrl, Path.GetFileName(DownloadUrl)) { DownloadFolder = DownloadFolder };
                downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                downloader.Download();
            }
            else
            {
                if (DownloadUrl.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(DownloadUrl, Path.Combine(DownloadFolder, Path.GetFileName(DownloadUrl)));
                }
                else
                {
                    var robo = new ExecutableUtil()
                    {
                        Executable = "robocopy.exe",
                        SuccessCodes = new[] { 0, 1 },
                        Args = $"\"{DownloadUrl}\" \"{DownloadFolder}\" /E /PURGE /XA:H"
                    };
                    var result = robo.Run();
                    if (!result.WasSuccess)
                    {
                        throw new System.Exception(result.Output);
                    }
                }
            }
        }

        private void UnZip()
        {
            if (DownloadUrl.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase))
            {
                var filename = Path.GetFileName(DownloadUrl);
                var zip = Path.Combine(DownloadFolder, filename);
                Logger?.Info("압축해재 시작");
                ZipFile.ExtractToDirectory(zip, Path.Combine(Path.GetDirectoryName(zip), Path.GetFileNameWithoutExtension(zip)));
                Logger?.Info("압축해재 종료");
            }
        }

        private void Deploy()
        {
            string source;
            if (DownloadUrl.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase))
            {
                var filename = Path.GetFileName(DownloadUrl);
                source = Path.Combine(DownloadFolder, Path.GetFileNameWithoutExtension(filename));
            }
            else
            {
                source = DownloadFolder;
            }

            Logger?.Info("설치시작");
            var installer = new CcnetPluginInstaller(source, PluginDirectory, ServiceDirecotry);
            installer.Install();
            Logger?.Info("설치종료");
        }

        private void Downloader_DownloadProgressChanged(object sender, AssetDownloader.AssertDownloadEventArgs e)
        {
            Logger?.Info($"download {e.ProgressPercentage}%");
        }

        private void StopService()
        {
            using (var ccnet = new CCNET())
            {
                Logger?.Info("stopping CCNET service");
                ccnet.StopService();
                Logger?.Info("stopped CCNET service");
            }
        }

        private void StartService()
        {
            using (var ccnet = new CCNET())
            {
                Logger?.Info("starting CCNET service");
                ccnet.StartService();
                Logger?.Info("started CCNET service");
            }
        }
    }
}