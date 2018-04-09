using System.Collections.Generic;
using System.Linq;
using TqLib.ccnet.Local;

namespace TqCcnetDashboard.Controllers.Api
{
    public class SourceControlTemplateController : AbstractApiController
    {
        public IEnumerable<object> Get(string id)
        {
            var type = CCNET.PluginReflectorTypes.FirstOrDefault(t => id.Equals(t.PluginName));
            if (type != null)
            {
                return type.PropertyInfos.Select(t => new
                {
                    propTypeName = t.PropertyTypeName,
                    attr = new
                    {
                        t.Attribute.Description,
                        t.Attribute.Name,
                        t.Attribute.Required,
                        DataType = new
                        {
                            t.Attribute.DataType.CustomDataType,
                            t.Attribute.DataType.DataType,
                            t.Attribute.DataType.Enums
                        }
                    }
                });
            }
            else
            {
                return new object[] { };
            }
        }
    }
}