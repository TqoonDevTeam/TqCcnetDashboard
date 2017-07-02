using Exortech.NetReflector;
using Exortech.NetReflector.Util;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace TqLib.ccnet.Core.Util
{
    public class StringJTokenSerializer : XmlMemberSerialiser
    {
        public StringJTokenSerializer(ReflectorMember member, ReflectorPropertyAttribute attribute) : base(member, attribute)
        {
        }

        public override object Read(XmlNode node, NetReflectorTypeTable table)
        {
            if (node == null)
            {
                return null;
            }
            else
            {
                return JToken.Parse(node.InnerText);
            }
        }

        public override void Write(XmlWriter writer, object target)
        {
            if (target == null)
            {
                return;
            }
            if (!(target is JToken))
            {
                target = base.ReflectorMember.GetValue(target);
            }
            JToken token = target as JToken;
            writer.WriteElementString(base.Attribute.Name, token.ToString());
        }
    }
}