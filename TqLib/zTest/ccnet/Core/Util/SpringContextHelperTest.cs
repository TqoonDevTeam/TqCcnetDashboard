using NUnit.Framework;
using TqLib.ccnet.Core.Util;

namespace TqLib.zTest.ccnet.Core.Util
{
    public class SpringContextHelperTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var ado = SpringContextHelper.GetAdoTemplate("System.Data.SqlClient", "SERVER=192.168.1.45 ;DATABASE=adprintNewDB;USER ID=web; PASSWORD=xlzns!@#0");
            var list = ado.QueryWithRowMapper(System.Data.CommandType.Text, "SELECT * FROM Joiner", new StringDictionaryMapper());
        }
    }
}