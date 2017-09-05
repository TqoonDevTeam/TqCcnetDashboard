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

                var tqBindings = lines.Select(t => t.Split(':'))
                    .Where(t => t.Length >= 3)
                    .Select(t => new TqBinding {
                        Ip = t[0].Trim(),
                        Port = Convert.ToInt32(t[1].Trim()),
                        Host = t[2].Trim(),
                        SSL = t.Length == 4 ? t[3].Trim() : string.Empty
                    });

                tqBindings.ToList().ForEach(t => {
                    bindins.Add(t);
                });
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