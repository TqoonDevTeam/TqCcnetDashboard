using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TqLib.ccnet.Utils
{
    public class CcnetPluginLocationFinder
    {
        public string FindPluginDirectory(string serviceDirectory)
        {
            var config = Path.Combine(serviceDirectory, "ccservice.exe.config");
            var doc = XDocument.Load(config);
            var pluginAbs = doc.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(t => t.Attribute("key").Value == "PluginLocation").Attribute("value").Value;
            if (string.IsNullOrEmpty(pluginAbs)) pluginAbs = "../plugin";
            return new Uri(new Uri(serviceDirectory + "\\"), pluginAbs).LocalPath;
        }
    }
}