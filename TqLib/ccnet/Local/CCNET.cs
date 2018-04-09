using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.ccnet.Utils;
using TqLib.Dashboard;

namespace TqLib.ccnet.Local
{
    public class CCNET : IDisposable
    {
        private static PluginTypeInfo[] pluginReflectorTypes, taskPluginReflectorTypes, publishPluginsReflectorTypes;
        private static ICruiseServerClientFactory fac = new CruiseServerClientFactory();

        public static PluginTypeInfo[] PluginReflectorTypes
        {
            get
            {
                if (pluginReflectorTypes == null)
                {
                    var ccnetPluginFInder = new CcnetPluginFInder(ServiceDirectory, PluginDirectory);
                    pluginReflectorTypes = ccnetPluginFInder.GetPluginTypeInfo();
                }
                return pluginReflectorTypes;
            }
        }

        public static PluginTypeInfo[] ScPluginsReflectorTypes
        {
            get
            {
                if (taskPluginReflectorTypes == null)
                {
                    taskPluginReflectorTypes = PluginReflectorTypes.Where(t =>
                    {
                        if (t.Namespace.StartsWith("ThoughtWorks"))
                        {
                            return t.Namespace == "ThoughtWorks.CruiseControl.Core.Sourcecontrol";
                        }
                        return true;
                    }).ToArray();
                }
                return taskPluginReflectorTypes;
            }
        }

        public static PluginTypeInfo[] TaskPluginsReflectorTypes
        {
            get
            {
                if (taskPluginReflectorTypes == null)
                {
                    taskPluginReflectorTypes = PluginReflectorTypes.Where(t =>
                     {
                         if (t.Namespace.StartsWith("ThoughtWorks"))
                         {
                             return t.Namespace == "ThoughtWorks.CruiseControl.Core.Tasks";
                         }
                         return true;
                     }).ToArray();
                }
                return taskPluginReflectorTypes;
            }
        }

        public static PluginTypeInfo[] PublishPluginsReflectorTypes
        {
            get
            {
                if (publishPluginsReflectorTypes == null)
                {
                    publishPluginsReflectorTypes = PluginReflectorTypes.Where(t => t.Namespace.EndsWith("Publishers")).ToArray();
                }
                return publishPluginsReflectorTypes;
            }
        }

        public CruiseServerClientBase Server { get; private set; }
        public static string ServiceDirectory { get; set; }
        public static string PluginDirectory { get; set; }

        static CCNET()
        {
            ServiceDirectory = new CcnetServiceLocationFinder().FindServiceDirectory();
            PluginDirectory = new CcnetPluginLocationFinder().FindPluginDirectory(ServiceDirectory);
        }

        public CCNET(string host = "127.0.0.1")
        {
            Server = fac.GenerateClient($"tcp://{host}:21234/CruiseManager.rem");
        }

        public void WaitDeleteComplete(string id)
        {
            using (var task = new Task(() =>
            {
                while (ExistsProject(id))
                {
                    Thread.Sleep(1000);
                }
            }))
            {
                task.Start();
                task.Wait(60000);
            }
        }

        public void WaitAddComplete(string id)
        {
            using (var task = new Task(() =>
            {
                while (!ExistsProject(id))
                {
                    Thread.Sleep(1000);
                }
            }))
            {
                task.Start();
                task.Wait(60000);
            }
        }

        public bool ExistsProject(string id)
        {
            return Server.GetProjectStatus().Any(t => t.Name == id);
        }

        public void StopService()
        {
            using (var svc = ServiceController.GetServices().First(t => "CCService".Equals(t.ServiceName)))
            {
                if (svc.Status == ServiceControllerStatus.Running)
                {
                    svc.Stop();
                    try
                    {
                        svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(120));
                    }
                    finally
                    {
                        var proc = Process.GetProcessesByName("CCService");
                        if (proc.Length > 0)
                        {
                            proc[0].Kill();
                        }
                    }
                }
            }
        }

        public void StartService()
        {
            using (var svc = ServiceController.GetServices().First(t => "CCService".Equals(t.ServiceName)))
            {
                if (svc.Status == ServiceControllerStatus.Stopped)
                {
                    svc.Start();
                    svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                }
            }
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}