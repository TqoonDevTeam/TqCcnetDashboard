using Exortech.NetReflector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThoughtWorks.CruiseControl.Core.Tasks;
using TqLib.ccnet.Utils;

namespace TqLib.Dashboard
{
    public class CcnetPluginFInder
    {
        public static readonly string TqDashboardDefaultPluginFileName = "ccnet.TqLib.plugin.dll";
        private string CcnetServiceDirecotry, CcnetPluginDirectory;

        public CcnetPluginFInder(string ccnetServiceDirectory, string ccnetPluginDirectory)
        {
            CcnetServiceDirecotry = ccnetServiceDirectory;
            CcnetPluginDirectory = ccnetPluginDirectory;
        }

        public IList<string> GetExternalPlugins()
        {
            return Directory.GetFiles(CcnetPluginDirectory, "*.dll")
                .Select(t => Path.GetFileName(t))
                .Except(new[] { TqDashboardDefaultPluginFileName }).ToList();
        }

        public IList<ExternalPluginsInfo> GetExternalPluginsInfo()
        {
            return GetExternalPlugins().Select(t =>
            new ExternalPluginsInfo()
            {
                Name = Path.GetFileNameWithoutExtension(t),
                UpdateSupport = false
            }
            ).ToList();
        }

        public PluginTypeInfo[] GetPluginTypeInfo()
        {
            var types = GetPluginTypes_ccnet();
            types = types.Union(GetPluginTypes_custom()).ToArray();
            return types;
        }

        private PluginTypeInfo[] GetPluginTypes_ccnet()
        {
            return typeof(TaskBase).Assembly.GetTypes().Where(t => t.IsDefined(typeof(ReflectorTypeAttribute), false)).Select(t => new PluginTypeInfo(t)).ToArray();
        }

        private PluginTypeInfo[] GetPluginTypes_custom()
        {
            var dir = new DirectoryInfo(CcnetPluginDirectory);
            if (dir.Exists)
            {
                return dir.GetFiles("*.dll").Select(t => t.FullName).SelectMany(t => GetPluginTypeInfo(t)).ToArray();
            }
            else
            {
                return new PluginTypeInfo[] { };
            }
        }

        private IList<PluginTypeInfo> GetPluginTypeInfo(string dllPath)
        {
            if (File.Exists(dllPath))
            {
                var typeLoader = new SeperateAppDomainAssemblyLoader() { CCNETServiceDirectory = CcnetServiceDirecotry };
                return typeLoader.GetAssemblyTypes(new FileInfo(dllPath)).ToList();
            }
            else
            {
                return new List<PluginTypeInfo>();
            }
        }
    }
}