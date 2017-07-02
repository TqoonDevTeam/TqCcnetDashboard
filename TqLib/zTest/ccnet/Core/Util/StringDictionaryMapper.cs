using Spring.Data.Generic;
using System.Collections.Generic;
using System.Data;

namespace TqLib.zTest.ccnet.Core.Util
{
    public class StringDictionaryMapper : IRowMapper<Dictionary<string, object>>
    {
        public Dictionary<string, object> MapRow(IDataReader reader, int rowNum)
        {
            var dic = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dic[reader.GetName(i)] = reader.GetValue(i);
            }
            return dic;
        }
    }
}