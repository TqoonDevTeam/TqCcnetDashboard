using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TqLib.ccnet.Utils
{
    public class CcnetServiceConfigInitializer
    {
        private const string assemblyBindingNamespace = "urn:schemas-microsoft-com:asm.v1";
        private string ServiceDirectory;

        public CcnetServiceConfigInitializer(string serviceDirectory)
        {
            ServiceDirectory = serviceDirectory;
        }

        public void Initialize()
        {
            foreach (var config in GetServiceConfigPath())
            {
                Update(config);
            }
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
            doc.Save(path);
        }

        private string[] GetServiceConfigPath()
        {
            var debugConfig = Path.Combine(ServiceDirectory, "ccnet.exe.config");
            var serviceConfig = Path.Combine(ServiceDirectory, "ccservice.exe.config");
            return new[] { debugConfig, serviceConfig };
        }
    }
}