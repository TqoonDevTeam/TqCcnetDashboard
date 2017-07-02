using Exortech.NetReflector;
using Exortech.NetReflector.Util;

namespace TqLib.ccnet.Core.Util
{
    public class SiteConfigSerializerFactory : ISerialiserFactory
    {
        public IXmlMemberSerialiser Create(ReflectorMember memberInfo, ReflectorPropertyAttribute attribute)
        {
            return new SiteConfigSerializer(memberInfo, attribute);
        }
    }
}