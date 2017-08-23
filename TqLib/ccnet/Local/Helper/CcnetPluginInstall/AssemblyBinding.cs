using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TqLib.ccnet.Local.Helper.CcnetPluginInstall
{
    [XmlRoot("assemblyBinding")]
    public class AssemblyBinding
    {
        private const string @namespace = "urn:schemas-microsoft-com:asm.v1";
        private string path;
        private XDocument doc;

        [XmlElement]
        public List<DependentAssembly> dependentAssembly { get; private set; }

        public void Load(string path)
        {
            this.path = path;
            this.doc = XDocument.Load(path);

            var assemblyBindingText = GetAssemblyBindingText();

            if (!string.IsNullOrEmpty(assemblyBindingText))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<DependentAssembly>), new XmlRootAttribute { ElementName = "assemblyBinding", Namespace = @namespace });
                using (var tr = new StringReader(assemblyBindingText))
                {
                    dependentAssembly = serializer.Deserialize(tr) as List<DependentAssembly>;
                }
            }
            else
            {
                dependentAssembly = new List<DependentAssembly>();
            }
        }

        public void Clear()
        {
            dependentAssembly = dependentAssembly.Where(t => t.codeBase == null).ToList();
        }

        public void Append(DependentAssembly item)
        {
            var old = dependentAssembly.FirstOrDefault(t => t.assemblyIdentity.name == item.assemblyIdentity.name && t.assemblyIdentity.publicKeyToken == item.assemblyIdentity.publicKeyToken);
            if (old == null)
            {
                dependentAssembly.Add(item);
            }
            else
            {
                if (old.codeBase != null)
                {
                    if (old.bindingRedirect.newVersion.CompareTo(item.bindingRedirect.newVersion) < 0)
                    {
                        dependentAssembly.Remove(old);
                        dependentAssembly.Add(item);
                    }
                }
            }
        }

        public void Save()
        {
            UpdateXDocumentDependentAssembly();
            doc.Save(path);
        }

        private void UpdateXDocumentDependentAssembly()
        {
            XElement assemblyBindingElement;
            XmlSerializer serializer = new XmlSerializer(typeof(AssemblyBinding), @namespace);
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, this);
                assemblyBindingElement = XElement.Parse(sw.ToString());
            }

            assemblyBindingElement.Attributes().Where(t => t.IsNamespaceDeclaration).Remove();

            doc.Element("configuration").Element("runtime").Element(XName.Get("assemblyBinding", @namespace)).Remove();
            doc.Element("configuration").Element("runtime").Add(assemblyBindingElement);
        }

        private string GetAssemblyBindingText()
        {
            return doc.Element("configuration").Element("runtime").Element(XName.Get("assemblyBinding", @namespace)).ToString();
        }
    }

    [XmlType("dependentAssembly")]
    public class DependentAssembly
    {
        [XmlElement]
        public AssemblyIdentity assemblyIdentity { get; set; }

        [XmlElement]
        public BindingRedirect bindingRedirect { get; set; }

        [XmlElement]
        public CodeBase codeBase { get; set; }
    }

    public class AssemblyIdentity
    {
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string publicKeyToken { get; set; }

        [XmlAttribute]
        public string culture { get; set; }
    }

    public class BindingRedirect
    {
        [XmlAttribute]
        public string oldVersion { get; set; }

        [XmlAttribute]
        public string newVersion { get; set; }
    }

    public class CodeBase
    {
        [XmlAttribute]
        public string version { get; set; }

        [XmlAttribute]
        public string href { get; set; }
    }
}