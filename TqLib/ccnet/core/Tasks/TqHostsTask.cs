using Exortech.NetReflector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqHosts", Description = "Hosts")]
    public class TqHostsTask : TaskBase
    {
        private const string hostsPath = @"C:\Windows\System32\drivers\etc\hosts";

        [ReflectorProperty("ip")]
        public string Ip { get; set; }

        [ReflectorProperty("host")]
        public string Host { get; set; }

        protected override bool Execute(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask("Executing Hosts");

            var task = Task.FromResult<bool>(UpdateHosts(result, Host, Ip));
            task.Wait(TimeSpan.FromSeconds(60));

            return task.Result;
        }

        private bool UpdateHosts(IIntegrationResult result, string host, string ip)
        {
            using (var fs = GetFileStream(result))
            {
                if (fs == null) return false;
                HostsCollection hosts;
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    var text = sr.ReadToEnd();
                    hosts = new HostsCollection(text);

                    if (hosts[host] == ip)
                    {
                        return true;
                    }
                    else
                    {
                        hosts[host] = ip;

                        string hostsContent = hosts.ToString();
                        var contentBytes = Encoding.UTF8.GetBytes(hostsContent);
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(contentBytes.Length);
                        fs.Write(contentBytes, 0, contentBytes.Length);
                    }
                }
            }
            return true;
        }

        private FileStream GetFileStream(IIntegrationResult result)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    return new FileStream(hostsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096);
                }
                catch (Exception ex)
                {
                    result.AddTaskResult($"파일 열기를 실패 했습니다. 시도 : {numTries}, 사유 : {ex.ToString()}");
                    if (numTries > 10)
                    {
                        result.AddTaskResult($"파일 열기를 중단합니다. 최대 허용횟수 10을 초과하였습니다.");
                        return null;
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        public class HostsCollection
        {
            private readonly Regex regex = new Regex(@"(^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})(\s.+)?", RegexOptions.Singleline | RegexOptions.Compiled);

            private IList<Hosts> _hosts = new List<Hosts>();

            public string this[string host]
            {
                get
                {
                    return _hosts.FirstOrDefault(t => t.Host == host)?.Ip ?? string.Empty;
                }
                set
                {
                    var target = _hosts.FirstOrDefault(t => t.Host == host);
                    if (target == null)
                    {
                        target = new Hosts { Host = host, Ip = value };
                        _hosts.Add(target);
                    }
                    else
                    {
                        target.Ip = value;
                    }
                }
            }

            public HostsCollection(string text)
            {
                Match match;
                foreach (var str in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None))
                {
                    match = regex.Match(str);
                    if (match.Success)
                    {
                        _hosts.Add(new Hosts { Host = match.Groups[2].Value.Trim(), Ip = match.Groups[1].Value });
                    }
                    else
                    {
                        _hosts.Add(new Hosts { Ip = str });
                    }
                }
            }

            public class Hosts
            {
                public string Ip { get; set; }
                public string Host { get; set; }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(Ip ?? string.Empty);
                    if (!string.IsNullOrEmpty(Host))
                    {
                        sb.Append(" ");
                        sb.Append(Host);
                    }
                    return sb.ToString();
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _hosts.Count; i++)
                {
                    if (i > 0) sb.AppendLine();
                    sb.Append(_hosts[i]);
                }
                return sb.ToString();
            }
        }
    }
}