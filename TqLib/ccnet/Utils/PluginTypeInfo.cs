using Exortech.NetReflector;
using System;
using System.Linq;
using System.Reflection;

namespace TqLib.ccnet.Utils
{
    [Serializable]
    public class PluginTypeInfo
    {
        public string PluginName { get; private set; }
        public string Namespace { get; private set; }

        public PropertyAttributeInfo[] PropertyInfos { get; private set; }

        public PluginTypeInfo(Type pluginType)
        {
            PluginName = GetPluginName(pluginType);
            Namespace = pluginType.Namespace;
            PropertyInfos = pluginType.GetProperties().Where(t => SeperateAppDomainAssemblyLoader.IsDefinedAttributeData<ReflectorPropertyAttribute>(t)).Select(t => new PropertyAttributeInfo(t)).ToArray();
        }

        private string GetPluginName(Type pluginType)
        {
            var attr = pluginType.GetCustomAttributesData().First(t => typeof(ReflectorTypeAttribute).Equals(t.AttributeType));
            return attr.ConstructorArguments[0].Value.ToString();
        }

        [Serializable]
        public class PropertyAttributeInfo
        {
            public string PropertyTypeName { get; private set; }
            public AttributeData Attribute { get; private set; }

            public PropertyAttributeInfo(PropertyInfo prop)
            {
                PropertyTypeName = prop.PropertyType.Name;
                var attrData = prop.GetCustomAttributesData().FirstOrDefault(t => typeof(ReflectorPropertyAttribute).Equals(t.AttributeType));
                Attribute = new AttributeData();
                Attribute.Name = attrData.ConstructorArguments[0].Value.ToString();
                Attribute.Description = attrData.NamedArguments.FirstOrDefault(t => t.MemberName == "Description").TypedValue.Value?.ToString() ?? string.Empty;
                Attribute.Required = (bool)(attrData.NamedArguments.FirstOrDefault(t => t.MemberName == "Required").TypedValue.Value ?? true);
            }

            [Serializable]
            public class AttributeData
            {
                public string Description { get; set; }
                public string Name { get; set; }
                public bool Required { get; set; }
            }
        }
    }
}