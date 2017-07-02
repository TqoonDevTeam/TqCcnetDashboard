using Exortech.NetReflector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public Type[] GetPluginTypes()
        {
            var types = GetPluginTypes_ccnet();
            types = types.Union(GetPluginTypes_custom()).ToArray();

            return types.Distinct().ToArray();
        }

        private Type[] GetPluginTypes_ccnet()
        {
            return typeof(TaskBase).Assembly.GetTypes().Where(t => t.IsDefined(typeof(ReflectorTypeAttribute), false)).ToArray();
        }

        private Type[] GetPluginTypes_custom()
        {
            var dir = new DirectoryInfo(CcnetPluginDirectory);
            if (dir.Exists)
            {
                return dir.GetFiles("*.dll").Select(t => t.FullName).SelectMany(t => GetAssemblyTypes(t)).ToArray();
            }
            else
            {
                return new Type[] { };
            }
        }

        private IList<Type> GetAssemblyTypes(string dllPath)
        {
            if (File.Exists(dllPath))
            {
                AppDomain dom = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.CodeBase = dllPath;
                Assembly assembly = dom.Load(assemblyName);
                var types = assembly.GetTypes().Where(t => t.IsDefined(typeof(ReflectorTypeAttribute))).ToList();
                AppDomain.Unload(dom);
                return types;
            }
            else
            {
                return new List<Type>();
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