using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace TqLib.ccnet.Core
{
    public class SerializedProjectToWebJObjectConverter
    {
        public JObject Convert(string serializedProject)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(serializedProject);
            XDocument xdoc = XDocument.Parse(doc.OuterXml);
            var project = xdoc.Element("project");

            ReplaceValidXElement(project, "publishers");
            ReplaceValidXElement(project, "prebuild");
            ReplaceValidXElement_Tasks(project);
            ReplaceValidXElement_SourceControl(project.Element("sourcecontrol"));
            ReplaceValidXElement_Triggers(project);

            doc.LoadXml(xdoc.ToString());

            var jObject = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
            var jProject = jObject["project"];
            ReplaceValidJArray(jProject, "publishers");
            ReplaceValidJArray(jProject, "prebuild");
            ReplaceValidJArray_Tasks(jProject);
            ReplaceValidJArray_SourceControl(jProject, "sourcecontrol");
            ReplaceValidJArray_Triggers(jProject);

            return jObject;
        }

        private void ReplaceValidXElement(XElement element, string elementName)
        {
            var list = GetValidXElement(element, elementName);
            element.Elements(elementName).Remove();
            element.Add(list);
        }

        private void ReplaceValidXElement_SourceControl(XElement e)
        {
            var nullSourceControl = "nullSourceControl";

            if (nullSourceControl.Equals(e.Attribute("type")?.Value))
            {
                e.Remove();
            }
            else if ("multi".Equals(e.Attribute("type")?.Value))
            {
                var list = GetValidXElement(e, "sourceControls");
                IList<XElement> removeTarget = new List<XElement>();
                foreach (var el in list)
                {
                    if (nullSourceControl.Equals(el.Attribute("type")?.Value))
                    {
                        removeTarget.Add(el);
                    }
                    else if ("multi".Equals(el.Attribute("type")?.Value))
                    {
                        ReplaceValidXElement_SourceControl(el);
                        if (el.Elements("sourceControls").Count() == 0)
                        {
                            removeTarget.Add(el);
                        }
                    }
                }
                removeTarget.Remove();
                e.Elements("sourceControls").Remove();
                e.Add(list);
            }
        }

        private void ReplaceValidXElement_Triggers(XElement e)
        {
            ReplaceValidXElement(e, "triggers");
            foreach (var el in e.Elements("triggers"))
            {
                if ("multiTrigger".Equals(el.Attribute("type")?.Value))
                {
                    ReplaceValidXElement_Triggers(el);
                }
            }
        }

        private void ReplaceValidXElement_Tasks(XElement e)
        {
            ReplaceValidXElement(e, "tasks");
            foreach (var el in e.Elements("tasks"))
            {
                if (el.Element("tasks") != null)
                {
                    ReplaceValidXElement_Tasks(el);
                }
            }
        }

        private IList<XElement> GetValidXElement(XElement element, string elementName)
        {
            XElement newE;
            IList<XElement> list = new List<XElement>();
            var array = new JArray();
            foreach (var e in element.Elements(elementName).Elements())
            {
                newE = new XElement(elementName);
                newE.SetAttributeValue("type", e.Name);
                newE.Add(e.Elements());
                list.Add(newE);
            }
            return list;
        }

        private void ReplaceValidJArray(JToken project, string key)
        {
            var token = project[key];
            if (token == null) return;
            if (token is JArray) return;
            project[key] = new JArray(token);
        }

        private void ReplaceValidJArray_SourceControl(JToken project, string key)
        {
            var token = string.IsNullOrEmpty(key) ? project : project[key];
            if (token == null) return;
            if (token is JArray) return;

            if ("multi".Equals(token["@type"]?.Value<string>()))
            {
                ReplaceValidJArray(token, "sourceControls");
                foreach (JToken jt in token["sourceControls"] as JArray)
                {
                    if ("multi".Equals(jt["@type"]?.Value<string>()))
                    {
                        ReplaceValidJArray_SourceControl(jt, "");
                    }
                }
            }
        }

        private void ReplaceValidJArray_Triggers(JToken project)
        {
            ReplaceValidJArray(project, "triggers");
            if (project["triggers"] == null) return;
            foreach (JToken jt in project["triggers"] as JArray)
            {
                if ("multiTrigger".Equals(jt["@type"]?.Value<string>()))
                {
                    ReplaceValidJArray_Triggers(jt);
                }
            }
        }

        private void ReplaceValidJArray_Tasks(JToken project)
        {
            ReplaceValidJArray(project, "tasks");
            if (project["tasks"] == null) return;
            foreach (JToken jt in project["tasks"] as JArray)
            {
                if (jt["tasks"] != null)
                {
                    ReplaceValidJArray_Tasks(jt);
                }
            }
        }
    }

    public class WebJObjectToSerializedProjectConverter
    {
        private bool ImportDefaultPublishers { get; set; }

        public string Convert(JObject jObject, bool importDefaultPublishers = true)
        {
            ImportDefaultPublishers = importDefaultPublishers;

            XmlDocument doc = JsonConvert.DeserializeXmlNode(jObject.ToString());

            XDocument xdoc = XDocument.Parse(doc.OuterXml);
            XElement project = xdoc.Element("project");

            ReplaceValidXElement(project, "publishers");
            ReplaceValidXElement(project, "prebuild");
            ReplaceValidXElement_Tasks(project);
            ReplaceValidXElement_SourceControl(project.Element("sourcecontrol"));
            ReplaceValidXElement_Triggers(project);

            SetDefaultValues(project);

            return xdoc.ToString(SaveOptions.None);
        }

        private void SetDefaultValues(XElement project)
        {
            if (ImportDefaultPublishers)
            {
                var publishers = project.Element("publishers");
                if (publishers == null)
                {
                    publishers = new XElement("publishers");
                    project.Add(publishers);
                }
                if (publishers.Element("xmllogger") == null) publishers.Add(new XElement("xmllogger"));
                if (publishers.Element("artifactcleanup") == null) publishers.Add(new XElement("artifactcleanup", new XAttribute("cleanUpMethod", "KeepLastXBuilds"), new XAttribute("cleanUpValue", "20")));
            }
        }

        private void ReplaceValidXElement(XElement element, string elementName)
        {
            var tasks = GetReplaceValidXElement(element, elementName);
            element.Elements(elementName).Remove();
            element.Add(tasks);
        }

        private void ReplaceValidXElement_SourceControl(XElement element)
        {
            if (element != null)
            {
                if ("multi".Equals(element.Attribute("type")?.Value) || "multi".Equals(element.Name.LocalName))
                {
                    ReplaceValidXElement(element, "sourceControls");

                    foreach (var el in element.Element("sourceControls").Elements())
                    {
                        ReplaceValidXElement_SourceControl(el);
                    }
                }
            }
        }

        private void ReplaceValidXElement_Triggers(XElement element)
        {
            ReplaceValidXElement(element, "triggers");
            foreach (var el in element.Element("triggers").Elements())
            {
                if ("multiTrigger".Equals(el.Name?.LocalName))
                {
                    ReplaceValidXElement_Triggers(el);
                }
            }
        }

        private void ReplaceValidXElement_Tasks(XElement element)
        {
            ReplaceValidXElement(element, "tasks");
            foreach (var el in element.Element("tasks").Elements())
            {
                if (el.Element("tasks") != null)
                {
                    ReplaceValidXElement_Tasks(el);
                }
            }
        }

        private XElement GetReplaceValidXElement(XElement element, string elementName)
        {
            string type;
            XElement tasks = new XElement(elementName);
            foreach (var e in element.Elements(elementName))
            {
                type = e.Attribute("type").Value;
                tasks.Add(new XElement(type, e.Elements()));
            }
            return tasks;
        }
    }

    public static class ProjectWebJsonConverter
    {
        public static readonly SerializedProjectToWebJObjectConverter SerializedProjectToWebJObjectConverter = new SerializedProjectToWebJObjectConverter();
        public static readonly WebJObjectToSerializedProjectConverter WebJObjectToSerializedProjectConverter = new WebJObjectToSerializedProjectConverter();
    }
}