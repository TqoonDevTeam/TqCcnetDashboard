using Exortech.NetReflector;
using Exortech.NetReflector.Util;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace TqLib.ccnet.Core.Util
{
    public class ApplyDbDataUtil
    {
        private readonly static Regex regex = new Regex(@"@\[db:([^]]+)\]");
        private readonly static NetReflectorTypeTable table = NetReflectorTypeTable.CreateDefault();

        private IList<ReflectorPropertyXmlChangeInfo> ReflectorPropertyXmlChangeInfoList { get; set; } = new List<ReflectorPropertyXmlChangeInfo>();

        public void ApplyDbData(object obj, Dictionary<string, object> dbData)
        {
            string replaceOldValue, replaceNewValue, dbColumnName;
            bool matchFound;
            MatchCollection matches;
            XmlDocument doc = new XmlDocument();

            foreach (var prop in GetReflectorPropertyUsedPropertyInfoList(obj))
            {
                matchFound = false;

                replaceOldValue = GetReflectorPropertyXmlData(obj, prop.MemberSerialiser);
                replaceNewValue = replaceOldValue;

                matches = regex.Matches(replaceOldValue);
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        dbColumnName = match.Groups[1].Value;
                        if (dbData.ContainsKey(dbColumnName))
                        {
                            replaceNewValue = replaceNewValue.Replace(match.Groups[0].Value, dbData[dbColumnName].ToString());
                        }
                        matchFound = true;
                    }
                }

                if (matchFound)
                {
                    doc.LoadXml(replaceNewValue);
                    prop.PropertyInfo.SetValue(obj, prop.MemberSerialiser.Read(doc.FirstChild, table));
                    ReflectorPropertyXmlChangeInfoList.Add(new ReflectorPropertyXmlChangeInfo
                    {
                        PropertyInfo = prop.PropertyInfo,
                        ReplaceOldValue = replaceOldValue,
                        ReplaceNewValue = replaceNewValue,
                        MemberSerialiser = prop.MemberSerialiser
                    });
                }
            }
        }

        public void RestoreDbData(object obj, Dictionary<string, object> dbData)
        {
            XmlDocument doc = new XmlDocument();
            foreach (var info in ReflectorPropertyXmlChangeInfoList)
            {
                doc.LoadXml(info.ReplaceOldValue);
                info.PropertyInfo.SetValue(obj, info.MemberSerialiser.Read(doc.FirstChild, table));
            }
        }

        private IEnumerable<ReflectorPropertyUsedPropertyInfo> GetReflectorPropertyUsedPropertyInfoList(object obj)
        {
            var type = obj.GetType();
            return type.GetProperties().Where(t => t.IsDefined(typeof(ReflectorPropertyAttribute))).Select(t => new ReflectorPropertyUsedPropertyInfo(t));
        }

        private string GetReflectorPropertyXmlData(object obj, IXmlSerialiser memberSerialiser)
        {
            StringBuilder xml = new StringBuilder();
            using (var writer = XmlWriter.Create(xml, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                memberSerialiser.Write(writer, obj);
            }
            return xml.ToString();
        }

        private bool ExistsReflectorPropertyXmlChangeInfo(PropertyInfo prop)
        {
            return ReflectorPropertyXmlChangeInfoList.Any(t => t.PropertyInfo == prop);
        }

        private class ReflectorPropertyUsedPropertyInfo
        {
            public PropertyInfo PropertyInfo { get; private set; }
            public IXmlSerialiser MemberSerialiser { get; private set; }

            public ReflectorPropertyUsedPropertyInfo(PropertyInfo prop)
            {
                PropertyInfo = prop;
                MemberSerialiser = GetMemberSerialiser(prop);
            }

            private IXmlSerialiser GetMemberSerialiser(PropertyInfo prop)
            {
                var reflectorPropertyAttribute = prop.GetCustomAttribute<ReflectorPropertyAttribute>();
                return reflectorPropertyAttribute.CreateSerialiser(ReflectorMember.Create(prop));
            }
        }

        private class ReflectorPropertyXmlChangeInfo
        {
            public IXmlSerialiser MemberSerialiser { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public string ReplaceOldValue { get; set; }
            public string ReplaceNewValue { get; set; }
        }
    }
}