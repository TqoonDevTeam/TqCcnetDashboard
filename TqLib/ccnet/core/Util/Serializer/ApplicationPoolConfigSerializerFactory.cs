using Exortech.NetReflector;
using Exortech.NetReflector.Util;

namespace TqLib.ccnet.Core.Util
{
    public class ApplicationPoolConfigSerializerFactory : ISerialiserFactory
    {
        public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
        {
            return new ApplicationPoolConfigSerializer(memberInfo, attribute);
        }
    }
}