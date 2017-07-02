using Exortech.NetReflector;
using Exortech.NetReflector.Util;

namespace TqLib.ccnet.Core.Util
{
    public class VirtualDirectorySerializerFactory : ISerialiserFactory
    {
        public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
        {
            return new VirtualDirectorySerializer(memberInfo, attribute);
        }
    }
}