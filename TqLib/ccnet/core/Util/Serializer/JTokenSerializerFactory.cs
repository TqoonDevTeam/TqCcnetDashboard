using Exortech.NetReflector;
using Exortech.NetReflector.Util;

namespace TqLib.ccnet.Core.Util
{
    public class JTokenSerializerFactory : ISerialiserFactory
    {
        public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
        {
            if (attribute.InstanceType == null || attribute.InstanceType.Equals(typeof(string)))
            {
                return new StringJTokenSerializer(memberInfo, attribute);
            }
            else
            {
                throw new System.NotSupportedException(attribute.InstanceType.Name);
            }
        }
    }
}