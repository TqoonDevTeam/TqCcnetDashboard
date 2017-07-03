using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using TqLib.Utils;

namespace TqLib.ccnet.Local.Helper
{
    public class CcnetServiceConfigUpdator
    {
        private const string assemblyBindingNamespace = "urn:schemas-microsoft-com:asm.v1";
        private string ServiceDirectory;

        public CcnetServiceConfigUpdator(string serviceDirectory)
        {
            ServiceDirectory = serviceDirectory;
        }

        public void Update()
        {
            string config = PathUtil.Combine(ServiceDirectory, "ccservice.exe.config");
            string config_console = PathUtil.Combine(ServiceDirectory, "ccnet.exe.config");
            Update(config);
            Update(config_console);
        }

        public void UpdateDependancy(PluginDependency pluginDependency)
        {
            string config = PathUtil.Combine(ServiceDirectory, "ccservice.exe.config");
            string config_console = PathUtil.Combine(ServiceDirectory, "ccnet.exe.config");
            UpdatePluginDependancy(config, pluginDependency);
            UpdatePluginDependancy(config_console, pluginDependency);
        }

        private void Update(string path)
        {
            var doc = XDocument.Load(path);

            // pluginPath
            var elm = doc.Element("configuration").Element("appSettings").Elements().First(t => "PluginLocation".Equals(t.Attribute("key")?.Value));
            elm.SetAttributeValue("value", "../plugin");

            // runtimeVersion
            XElement supportedRuntime = XElement.Parse("<supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.6\" />");
            var elm_startup = doc.Element("configuration").Element("startup");
            if (elm_startup != null)
            {
                var startup = elm_startup.Element("supportedRuntime");
                if (startup != null) startup.Remove();
                elm_startup.Add(supportedRuntime);
            }
            else
            {
                doc.Element("configuration").Add(new XElement("startup", supportedRuntime));
            }

            DependentAssemblyUpdate(doc);

            doc.Save(path);
        }

        private void UpdatePluginDependancy(string path, PluginDependency pluginDependency)
        {
            var doc = XDocument.Load(path);

            // configuration
            XElement configuration = doc.Element("configuration");

            // runtime
            XElement runtime = FindElementIfNotExistsCreate(configuration, "runtime");

            // assemblyBinding
            XElement assemblyBinding = FindElementIfNotExistsCreate(runtime, "assemblyBinding", assemblyBindingNamespace);

            foreach (var p in pluginDependency.GetAllModuleInfos())
            {
                CheckAndAppendDependentAssembly(assemblyBinding, p.name, p.publicKeyToken, p.culture, p.version);
            }

            doc.Save(path);
        }

        private void DependentAssemblyUpdate(XDocument doc)
        {
            // configuration
            XElement configuration = doc.Element("configuration");

            // runtime
            XElement runtime = FindElementIfNotExistsCreate(configuration, "runtime");

            // assemblyBinding
            XElement assemblyBinding = FindElementIfNotExistsCreate(runtime, "assemblyBinding", assemblyBindingNamespace);

            // dependentAssemblies

            // Common.Logging
            CheckAndAppendDependentAssembly(assemblyBinding, typeof(Common.Logging.LogManager).Assembly);
            // Common.Logging.Core
            CheckAndAppendDependentAssembly(assemblyBinding, typeof(Common.Logging.LogLevel).Assembly);
        }

        private XElement FindElementIfNotExistsCreate(XElement parent, string name, string _namespace = "")
        {
            var element = parent.Element(XName.Get(name, _namespace));
            if (element == null)
            {
                element = new XElement(name);
                parent.Add(element);
            }
            return element;
        }

        private XElement FindDependentAssembly(XElement assemblyBinding, string name, string publicKeyToken)
        {
            return assemblyBinding.Elements(XName.Get("dependentAssembly", assemblyBindingNamespace)).FirstOrDefault(t =>
            {
                var assemblyIdentity = t.Element(XName.Get("assemblyIdentity", assemblyBindingNamespace));
                if (assemblyIdentity != null)
                {
                    if (name.Equals(assemblyIdentity.Attribute("name")?.Value) && publicKeyToken.Equals(assemblyIdentity.Attribute("publicKeyToken")?.Value))
                    {
                        return true;
                    }
                }
                return false;
            });
        }

        private void CheckAndAppendDependentAssembly(XElement assemblyBinding, Assembly assembly)
        {
            AssemblyName assemblyName = assembly.GetName();
            string name = assemblyName.Name;
            string publicKeyToken = GetPublicKeyToken(assemblyName.GetPublicKeyToken());
            string culture = string.IsNullOrEmpty(assemblyName.CultureName) ? "neutral" : "assemblyName.CultureName";
            string version = assemblyName.Version.ToString();
            CheckAndAppendDependentAssembly(assemblyBinding, name, publicKeyToken, culture, version);
        }

        private void CheckAndAppendDependentAssembly(XElement assemblyBinding, string assemblyName, string publicKeyToken, string culture, string version)
        {
            var dependentAssembly = FindDependentAssembly(assemblyBinding, assemblyName, publicKeyToken);

            if (dependentAssembly == null)
            {
                dependentAssembly = new XElement(XName.Get("dependentAssembly", assemblyBindingNamespace),
                    new XElement(XName.Get("assemblyIdentity", assemblyBindingNamespace), new XAttribute("name", assemblyName), new XAttribute("publicKeyToken", publicKeyToken), new XAttribute("culture", culture)),
                    new XElement(XName.Get("bindingRedirect", assemblyBindingNamespace), new XAttribute("oldVersion", "0.0.0.0-99.9.9.9"), new XAttribute("newVersion", version)));
                assemblyBinding.Add(dependentAssembly);
            }
            else
            {
                var bindingRedirect = dependentAssembly.Element(XName.Get("bindingRedirect", assemblyBindingNamespace));
                bindingRedirect.SetAttributeValue("oldVersion", "0.0.0.0-99.9.9.9");
                bindingRedirect.SetAttributeValue("newVersion", version);
            }
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
    }
}