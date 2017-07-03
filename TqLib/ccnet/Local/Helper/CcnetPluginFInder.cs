using Exortech.NetReflector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace TqLib.ccnet.Local.Helper
{
    public class CcnetPluginFInder
    {
        public static readonly string TqDashboardDefaultPluginFileName = "ccnet.TqLib.plugin.dll";
        public string CcnetServiceDirecotry { get; private set; }
        public string CcnetPluginDirectory { get; private set; }

        public CcnetPluginFInder(string ccnetServiceDirecotry)
        {
            CcnetServiceDirecotry = ccnetServiceDirecotry;
            SetCcnetPluginDirectory();
        }

        public IList<string> GetExternalPlugins()
        {
            return Directory.GetFiles(CcnetPluginDirectory, "*.dll")
                .Select(t => Path.GetFileName(t))
                .Except(new[] { TqDashboardDefaultPluginFileName }).ToList();
        }

        public IList<ExternalDll> GetExternalPluginsWithInfo()
        {
            return Directory.GetFiles(CcnetPluginDirectory, "*.dll").Select(t => new ExternalDll(t)).Where(t => t.IsCcnetPlugin && t.FileName != CcnetPluginFInder.TqDashboardDefaultPluginFileName).ToList();
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
                //var typeofReflectorTypeAttribute = typeof(ReflectorTypeAttribute);
                //if (Constants.Key.DefaultPluginName.Equals(Path.GetFileNameWithoutExtension(dllPath)))
                //{
                //    return typeof(Constants).Assembly.GetTypes().Where(t => t.IsDefined(typeofReflectorTypeAttribute)).Select(t => new PluginTypeInfo(t)).ToList();
                //}
                //else
                //{
                //    var typeLoader = new SeperateAppDomainAssemblyLoader() { CCNETServiceDirectory = CCNET.ServiceDirectory };
                //    return typeLoader.GetAssemblyTypes(new FileInfo(dllPath)).ToList();
                //}
                var typeLoader = new SeperateAppDomainAssemblyLoader() { CCNETServiceDirectory = CCNET.ServiceDirectory };
                return typeLoader.GetAssemblyTypes(new FileInfo(dllPath)).ToList();
            }
            else
            {
                return new List<PluginTypeInfo>();
            }
        }

        private void SetCcnetPluginDirectory()
        {
            var config = Path.Combine(CcnetServiceDirecotry, "ccservice.exe.config");
            var doc = XDocument.Load(config);
            var pluginAbs = doc.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(t => t.Attribute("key").Value == "PluginLocation").Attribute("value").Value;
            CcnetPluginDirectory = new Uri(new Uri(CcnetServiceDirecotry + "\\"), pluginAbs).LocalPath;
        }
    }
}