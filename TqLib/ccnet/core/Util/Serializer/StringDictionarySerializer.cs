using Exortech.NetReflector;
using Exortech.NetReflector.Util;
using System.Collections.Generic;
using System.Xml;

namespace TqLib.ccnet.Core.Util
{
    public class StringDictionarySerializer : XmlMemberSerialiser
    {
        public StringDictionarySerializer(ReflectorMember member, ReflectorPropertyAttribute attribute) : base(member, attribute)
        {
        }

        public override object Read(XmlNode node, NetReflectorTypeTable table)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (node != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    dic[child.Name] = child.InnerText;
                }
            }
            return dic;
        }

        public override void Write(XmlWriter writer, object target)
        {
            if (target == null)
            {
                return;
            }
            if (!(target is Dictionary<string, string>))
            {
                target = base.ReflectorMember.GetValue(target);
            }

            Dictionary<string, string> dic = target as Dictionary<string, string>;
            writer.WriteStartElement(base.Attribute.Name);
            foreach (var kv in dic)
            {
                writer.WriteElementString(kv.Key, kv.Value.ToString());
            }
            writer.WriteEndElement();
        }
    }
}