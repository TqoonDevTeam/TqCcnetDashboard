using Exortech.NetReflector;
using Exortech.NetReflector.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TqLib.ccnet.Core.Util
{
    public class BindingsSerializer : XmlMemberSerialiser
    {
        public BindingsSerializer(ReflectorMember member, ReflectorPropertyAttribute attribute) : base(member, attribute)
        {
        }

        public override object Read(XmlNode node, NetReflectorTypeTable table)
        {
            IList<TqBinding> bindins = new List<TqBinding>();

            var str = node.InnerText;

            if (!string.IsNullOrEmpty(str))
            {
                string[] lines = str.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                string[] kv;
                foreach (var line in lines)
                {
                    kv = line.Split(':');

                    bindins.Add(new TqBinding()
                    {
                        Ip = kv[0],
                        Port = Convert.ToInt32(kv[1]),
                        Host = kv[2],
                        SSL = kv.Length == 4 ? kv[3] : string.Empty
                    });
                }
            }
            return bindins.ToArray();
        }

        public override void Write(XmlWriter writer, object target)
        {
            if (target == null)
            {
                return;
            }
            if (!(target is TqBinding[]))
            {
                target = base.ReflectorMember.GetValue(target);
            }

            TqBinding[] dic = target as TqBinding[];
            string value = string.Join("\n", dic.Select(t => t.ToString()));
            writer.WriteElementString(base.Attribute.Name, value);
        }
    }
}