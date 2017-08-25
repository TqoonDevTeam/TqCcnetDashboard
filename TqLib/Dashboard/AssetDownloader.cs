using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TqLib.Dashboard
{
    public class AssetDownloader
    {
        private string _FileName, _Url, _downloadPath;
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
}