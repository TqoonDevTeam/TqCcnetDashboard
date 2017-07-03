using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TqLib.ccnet.Local.Helper
{
    public class CcnetPluginInstaller
    {
        public string[] AllowExtensions { get; set; } = new[] { ".dll", ".exe" };
        public string[] DisAllowFIleNames { get; set; } = new string[] { };
        private string SrcDirectory, PluginDirectory, ServiceDirecotry;

        public CcnetPluginInstaller(string srcDirectory, string pluginDirectory, string serviceDirecotry)
        {
            SrcDirectory = srcDirectory;
            PluginDirectory = pluginDirectory;
            ServiceDirecotry = serviceDirecotry;
        }

        public void Install()
        {
            var files = Directory.GetFiles(SrcDirectory, "*.*").Where(file => AllowExtensions.Any(ext => ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))).Select(t => new ExternalDll(t)).ToList();
            var pluginFiles = files.Where(t => t.IsCcnetPlugin).ToList();
            var pluginReferenceFiles = files.Except(pluginFiles).ToList();

            PluginDependency pluginDependency = GetPluginDependency();

            foreach (var plugin in pluginFiles)
            {
                if (ExistsPluginInfo(pluginDependency, plugin.AssemblyName))
                {
                    UpdatePluginInfo(pluginDependency, plugin.AssemblyName, plugin.AssemblyVersion);
                }
                else
                {
                    CreatePluginInfo(pluginDependency, plugin.AssemblyName, plugin.AssemblyVersion);
                }

                File.Copy(plugin.FilePath, Path.Combine(PluginDirectory, plugin.FileName), true);
            }

            foreach (var pluginReference in pluginReferenceFiles)
            {
                if (DisAllowFIleNames.Contains(pluginReference.FileName)) continue;

                foreach (var plugin in pluginFiles)
                {
                    if (ExistsPluginReferenceModuleInfo(pluginDependency, plugin.AssemblyName, pluginReference.AssemblyName))
                    {
                        UpdatePluginReferenceModuleInfo(pluginDependency, plugin.AssemblyName, pluginReference);
                    }
                    else
                    {
                        CreatePluginReferenceModuleInfo(pluginDependency, plugin.AssemblyName, pluginReference);
                    }
                }

                if (IsNewVersion(pluginDependency, pluginReference))
                {
                    File.Copy(pluginReference.FilePath, Path.Combine(ServiceDirecotry, pluginReference.FileName), true);
                }
            }

            SavePluginDependency(pluginDependency);
            var configUpdator = new CcnetServiceConfigUpdator(ServiceDirecotry);
            configUpdator.UpdateDependancy(pluginDependency);
        }

        private PluginDependency GetPluginDependency()
        {
            string pluginDependencyXmlPath = GetPluginDependencyXmlPath();
            if (File.Exists(pluginDependencyXmlPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PluginDependency));
                    using (var sr = new StreamReader(pluginDependencyXmlPath))
                    {
                        return serializer.Deserialize(sr) as PluginDependency;
                    }
                }
                catch
                {
                    return new PluginDependency();
                }
            }
            else
            {
                return new PluginDependency();
            }
        }

        private void SavePluginDependency(PluginDependency pluginDependency)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PluginDependency));
            string xml;
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, pluginDependency);
                xml = textWriter.ToString();
            }
            var doc = XDocument.Parse(xml);
            doc.Save(GetPluginDependencyXmlPath());
        }

        private bool ExistsPluginInfo(PluginDependency pluginDependency, string assemblyName)
        {
            return pluginDependency.Plugin.Any(t => t.name.Equals(assemblyName));
        }

        private void UpdatePluginInfo(PluginDependency pluginDependency, string assemblyName, string assemblyVersion)
        {
            pluginDependency.Plugin.First(t => t.name.Equals(assemblyName)).version = assemblyVersion;
        }

        private void CreatePluginInfo(PluginDependency pluginDependency, string assemblyName, string assemblyVersion)
        {
            pluginDependency.Plugin.Add(new PluginDependency.PluginInfo() { name = assemblyName, version = assemblyVersion });
        }

        private bool ExistsPluginReferenceModuleInfo(PluginDependency pluginDependency, string pluginAssemblyName, string referenceAssemblyName)
        {
            return pluginDependency.Plugin.First(t => t.name.Equals(pluginAssemblyName)).Installed.Any(t => t.name.Equals(referenceAssemblyName));
        }

        private void UpdatePluginReferenceModuleInfo(PluginDependency pluginDependency, string pluginAssemblyName, ExternalDll dll)
        {
            pluginDependency.Plugin.First(t => t.name.Equals(pluginAssemblyName)).Installed.First(t => t.name.Equals(dll.AssemblyName)).version = dll.AssemblyVersion;
        }

        private void CreatePluginReferenceModuleInfo(PluginDependency pluginDependency, string pluginAssemblyName, ExternalDll dll)
        {
            pluginDependency.Plugin.First(t => t.name.Equals(pluginAssemblyName)).Installed.Add(new PluginDependency.ModuleInfo(dll));
        }

        private bool IsNewVersion(PluginDependency pluginDependency, ExternalDll dll)
        {
            var find = pluginDependency.GetAllModuleInfos().Where(t => t.name.Equals(dll.AssemblyName)).OrderByDescending(t => t.version).FirstOrDefault();
            if (find != null)
            {
                return !(new[] { find.version, dll.AssemblyVersion }.OrderByDescending(t => t).First().Equals(dll.AssemblyVersion));
            }
            else
            {
                return true;
            }
        }

        private string GetPluginDependencyXmlPath()
        {
            return Path.Combine(PluginDirectory, "PluginDependency.xml");
        }
    }

    [XmlRoot]
    public class PluginDependency
    {
        [XmlElement]
        public List<PluginInfo> Plugin { get; set; } = new List<PluginInfo>();

        public class PluginInfo
        {
            [XmlAttribute]
            public string name { get; set; }

            [XmlAttribute]
            public string version { get; set; }

            [XmlElement]
            public List<ModuleInfo> Installed { get; set; } = new List<ModuleInfo>();
        }

        public class ModuleInfo
        {
            [XmlAttribute]
            public string name { get; set; }

            [XmlAttribute]
            public string version { get; set; }

            [XmlAttribute]
            public string publicKeyToken { get; set; }

            [XmlAttribute]
            public string culture { get; set; }

            public ModuleInfo()
            {
            }

            public ModuleInfo(ExternalDll dll)
            {
                name = dll.AssemblyName;
                version = dll.AssemblyVersion;
                publicKeyToken = dll.PublicKeyToken;
                culture = dll.Culture;
            }
        }

        public IEnumerable<ModuleInfo> GetAllModuleInfos()
        {
            return Plugin.SelectMany(t => t.Installed);
        }
    }

    public class ExternalDll
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string AssemblyName { get; private set; }
        public string AssemblyVersion { get; private set; }
        public string PublicKeyToken { get; private set; }
        public string Culture { get; private set; }
        public bool IsCcnetPlugin { get; private set; }

        public ExternalDll(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);

            var assemblyName = System.Reflection.AssemblyName.GetAssemblyName(FilePath);
            AssemblyName = assemblyName.Name;
            AssemblyVersion = assemblyName.Version.ToString();
            PublicKeyToken = GetPublicKeyToken(assemblyName.GetPublicKeyToken());
            Culture = string.IsNullOrEmpty(assemblyName.CultureName) ? "neutral" : "assemblyName.CultureName";
            IsCcnetPlugin = IsPlugin(AssemblyName);
        }

        private string GetPublicKeyToken(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.GetLength(0); i++)
            {
                sb.AppendFormat("{0:x2}", bytes[i]);
            }
            return sb.ToString();
        }

        private bool IsPlugin(string shortAssemblyName)
        {
            return shortAssemblyName.StartsWith("ccnet.") && shortAssemblyName.EndsWith(".plugin");
        }
    }
}