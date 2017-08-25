using log4net;
using System.IO;
using System.IO.Compression;
using TqLib.Utils;

namespace TqLib.Dashboard
{
    public class DashboardUpdator
    {
        public ILog Logger { get; set; }
        public ILog SystemLogger { get; set; }
        public string DownloadFolder { get; set; }
        public string DashboardFolder { get; set; }
        public string DownloadUrl { get; set; } = "https://github.com/TqoonDevTeam/TqCcnetDashboard/releases/download/1.0.0.0/web.zip";

        public void Update()
        {
            if (DashboardFolder.EndsWith(@"\")) DashboardFolder = DashboardFolder.Substring(0, DashboardFolder.Length - 1);
            CheckDownloadFolder();
            Download();
            UnZip();
            Deploy();
        }

        private void CheckDownloadFolder()
        {
            Logger?.Info("DashboardUpdator - Download Folder Check");
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
                Logger?.Info("DashboardUpdator - UnZip");
                ZipFile.ExtractToDirectory(zip, Path.Combine(Path.GetDirectoryName(zip), Path.GetFileNameWithoutExtension(zip)));
            }
        }

        private void Deploy()
        {
            Logger?.Info("DashboardUpdator - deploying");
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

            var robo = new ExecutableUtil()
            {
                Executable = "robocopy.exe",
                SuccessCodes = new[] { 0, 1 },
                Args = $"\"{source}\" \"{DashboardFolder}\" /E /PURGE /XA:H /XD Config"
            };
            var result = robo.Run();
            if (result.WasSuccess)
            {
                Logger?.Info("DashboardUpdator - deploy success");
            }
            else
            {
                Logger?.Error("DashboardUpdator - deploy fail");
                SystemLogger?.Error("DashboardUpdator - deploy fail\n" + result.Output);
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, AssetDownloader.AssertDownloadEventArgs e)
        {
            Logger?.Info($"download {e.ProgressPercentage}%");
        }
    }
}