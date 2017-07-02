using Exortech.NetReflector;
using Exortech.NetReflector.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TqLib.ccnet.Core.Util
{
    public class VirtualDirectorySerializer : XmlMemberSerialiser
    {
        public VirtualDirectorySerializer(ReflectorMember member, ReflectorPropertyAttribute attribute) : base(member, attribute)
        {
        }

        public override object Read(XmlNode node, NetReflectorTypeTable table)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            if (node != null)
            {
                var str = node.InnerText;

                if (!string.IsNullOrEmpty(str))
                {
                    string[] lines = str.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    string[] kv;
                    foreach (var line in lines)
                    {
                        kv = line.Split('=');
                        config[kv[0]] = kv[1];
                    }
                }
            }
            return config;
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
            string value = string.Join("\n", dic.Select(t => t.Key + "=" + t.Value));
            writer.WriteElementString(base.Attribute.Name, value);
        }
    }
}