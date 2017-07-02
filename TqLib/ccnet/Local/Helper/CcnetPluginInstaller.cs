using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TqLib.ccnet.Local.Helper
{
    public class CcnetPluginInstaller
    {
        public string[] AllowExtensions { get; set; } = new[] { ".dll", ".exe" };
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

            XDocument pdoc = GetPluginDependencyXml();

            foreach (var plugin in pluginFiles)
            {
                if (ExistsPluginReferenceInfo(pdoc, plugin.AssemblyName))
                {
                    UpdatePluginReferenceInfo(pdoc, plugin.AssemblyName, plugin.AssemblyVersion);
                }
                else
                {
                    CreatePluginReferenceInfo(pdoc, plugin.AssemblyName, plugin.AssemblyVersion);
                }

                File.Copy(plugin.FilePath, Path.Combine(PluginDirectory, plugin.FileName), true);
            }

            foreach (var pluginReference in pluginReferenceFiles)
            {
                foreach (var plugin in pluginFiles)
                {
                    if (ExistsPluginReferenceModuleInfo(pdoc, plugin.AssemblyName, pluginReference.AssemblyName))
                    {
                        UpdatePluginReferenceModuleInfo(pdoc, plugin.AssemblyName, pluginReference.AssemblyName, pluginReference.AssemblyVersion);
                    }
                    else
                    {
                        CreatePluginReferenceModuleInfo(pdoc, plugin.AssemblyName, pluginReference.AssemblyName, pluginReference.AssemblyVersion);
                    }
                }
                File.Copy(pluginReference.FilePath, Path.Combine(ServiceDirecotry, pluginReference.FileName), true);
            }

            pdoc.Save(GetPluginDependencyXmlPath());
        }

        private XDocument GetPluginDependencyXml()
        {
            string pluginDependencyXmlPath = GetPluginDependencyXmlPath();
            if (File.Exists(pluginDependencyXmlPath))
            {
                return XDocument.Load(pluginDependencyXmlPath);
            }
            else
            {
                var doc = new XDocument();
                doc.Add(new XElement("PluginDependency"));
                return doc;
            }
        }

        private bool ExistsPluginReferenceInfo(XDocument pdoc, string assemblyName)
        {
            return GetPluginReferenceInfo(pdoc, assemblyName) != null;
        }

        private bool ExistsPluginReferenceModuleInfo(XDocument pdoc, string pluginAssemblyName, string referenceAssemblyName)
        {
            return GetPluginReferenceModuleInfo(pdoc, pluginAssemblyName, referenceAssemblyName) != null;
        }

        private void CreatePluginReferenceInfo(XDocument pdoc, string assemblyName, string assemblyVersion)
        {
            pdoc.Element("PluginDependency").Add(
                new XElement("PluginReferenceInfo",
                    new XAttribute("name", assemblyName),
                    new XAttribute("version", assemblyVersion)
                    )
                );
        }

        private void CreatePluginReferenceModuleInfo(XDocument pdoc, string pluginAssemblyName, string referenceAssemblyName, string referenceAssemblyVersion)
        {
            var element = GetPluginReferenceInfo(pdoc, pluginAssemblyName);
            element.Add(new XElement("Module",
                    new XAttribute("name", referenceAssemblyName),
                    new XAttribute("version", referenceAssemblyVersion)));
        }

        private void UpdatePluginReferenceInfo(XDocument pdoc, string assemblyName, string assemblyVersion)
        {
            var element = GetPluginReferenceInfo(pdoc, assemblyName);
            element.SetAttributeValue("version", assemblyVersion);
        }

        private void UpdatePluginReferenceModuleInfo(XDocument pdoc, string pluginAssemblyName, string referenceAssemblyName, string referenceAssemblyVersion)
        {
            var element = GetPluginReferenceModuleInfo(pdoc, pluginAssemblyName, referenceAssemblyName);
            element.SetAttributeValue("version", referenceAssemblyVersion);
        }

        private XElement GetPluginReferenceInfo(XDocument pdoc, string assemblyName)
        {
            return pdoc.Element("PluginDependency").Elements("PluginReferenceInfo").SingleOrDefault(t => assemblyName.Equals(t.Attribute("name")?.Value));
        }

        private XElement GetPluginReferenceModuleInfo(XDocument pdoc, string pluginAssemblyName, string referenceAssemblyName)
        {
            return GetPluginReferenceInfo(pdoc, pluginAssemblyName).Elements("Module").SingleOrDefault(t => referenceAssemblyName.Equals(t.Attribute("name")?.Value));
        }

        private string GetPluginDependencyXmlPath()
        {
            return Path.Combine(PluginDirectory, "PluginDependency.xml");
        }
    }

    public class ExternalDll
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string AssemblyName { get; private set; }
        public string AssemblyVersion { get; private set; }

        public bool IsCcnetPlugin { get; private set; }

        public ExternalDll(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);

            var assemblyName = System.Reflection.AssemblyName.GetAssemblyName(FilePath);

            AssemblyName = assemblyName.Name;
            AssemblyVersion = assemblyName.Version.ToString();
            IsCcnetPlugin = IsPlugin(AssemblyName);
        }

        private bool IsPlugin(string shortAssemblyName)
        {
            return shortAssemblyName.StartsWith("ccnet.") && shortAssemblyName.EndsWith(".plugin");
        }
    }
}