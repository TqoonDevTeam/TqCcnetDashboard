using Exortech.NetReflector;
using Exortech.NetReflector.Util;

namespace TqLib.ccnet.Core.Util
{
    public class BindingsSerializerFactory : ISerialiserFactory
    {
        public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
        {
            return new BindingsSerializer(memberInfo, attribute);
        }
    }
}