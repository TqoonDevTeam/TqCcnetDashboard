using System.IO;
using System.Xml.Linq;

namespace TqCcnetDashboard.Config
{
    public static class ConfigManager
    {
        private static string ConfigPath;
        private static XDocument config;

        static ConfigManager()
        {
            ConfigPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Config"), "config.xml");
            if (File.Exists(ConfigPath))
            {
                config = XDocument.Load(ConfigPath);
            }
            else
            {
                config = new XDocument(new XElement("configs"));
            }
        }

        public static string Get(string key, string defaultValue = null)
        {
            return GetElement(key)?.Value ?? defaultValue;
        }

        public static void Set(string key, string value)
        {
            var el = GetElement(key);
            if (el == null)
            {
                el = new XElement(key, value);
                config.Element("configs").Add(el);
            }
            else
            {
                el.Value = value;
            }
            config.Save(ConfigPath);
        }

        private static XElement GetElement(string key)
        {
            return config.Element("configs").Element(key);
        }
    }
}