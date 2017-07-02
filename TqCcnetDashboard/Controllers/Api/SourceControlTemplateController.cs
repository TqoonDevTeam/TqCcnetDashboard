using Exortech.NetReflector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TqLib.ccnet.Local;

namespace TqCcnetDashboard.Controllers.Api
{
    public class SourceControlTemplateController : AbstractApiController
    {
        // GET api/<controller>/5
        public IEnumerable<object> Get(string id)
        {
            ReflectorTypeAttribute attr;
            var type = CCNET.PluginReflectorTypes.FirstOrDefault(t =>
            {
                attr = t.GetCustomAttribute<ReflectorTypeAttribute>();
                return id.Equals(attr?.Name);
            });
            if (type != null)
            {
                return type.GetProperties().Where(t => t.IsDefined(typeof(ReflectorPropertyAttribute))).Select(t =>
                new
                {
                    propTypeName = t.PropertyType.Name,
                    attr = t.GetCustomAttribute<ReflectorPropertyAttribute>()
                });
            }
            else
            {
                return new object[] { };
            }
        }
    }
}