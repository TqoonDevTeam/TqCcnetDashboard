using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.ccnet.Core;
using TqLib.ccnet.Local;

namespace TqCcnetDashboard.Controllers.Api
{
    public class CcnetProjectController : AbstractApiController
    {
        public IEnumerable<ProjectStatus> Get()
        {
            using (var ccnet = new CCNET())
            {
                var list = ccnet.Server.GetProjectStatus().ToList();
                return list;
            }
        }

        public JObject Get(string id)
        {
            using (var ccnet = new CCNET())
            {
                string xmlConfig = ccnet.Server.GetProject(id);
                return ProjectWebJsonConverter.SerializedProjectToWebJObjectConverter.Convert(xmlConfig);
            }
        }

        public void Post(JObject value)
        {
            string json = $"{{ \"project\":{value.ToString(Newtonsoft.Json.Formatting.None)} }}";
            using (var ccnet = new CCNET())
            {
                JObject project = JObject.Parse(json);
                var xmlConfig = ProjectWebJsonConverter.WebJObjectToSerializedProjectConverter.Convert(project);
                ccnet.Server.AddProject(xmlConfig);
                ccnet.WaitAddComplete(value["name"].Value<string>());
            }
        }

        public object Post(string id, JObject value)
        {
            if (id == "wizard")
            {
                string json = $"{{ \"project\":{value.ToString(Newtonsoft.Json.Formatting.None)} }}";
                using (var ccnet = new CCNET())
                {
                    JObject project = JObject.Parse(json);
                    string projectName = value["name"].Value<string>();
                    var xmlConfig = ProjectWebJsonConverter.WebJObjectToSerializedProjectConverter.Convert(project);
                    if (ccnet.Server.GetProjectStatus().Any(t => t.Name == projectName))
                    {
                        return new { error = true, msg = "같은 이름의 프로젝트가 이미 등록되었습니다." };
                    }

                    ccnet.Server.AddProject(xmlConfig);
                    ccnet.WaitAddComplete(projectName);
                }
            }
            return new { error = false };
        }

        public void Put(string id, JObject value)
        {
            string json = $"{{ \"project\":{value.ToString(Newtonsoft.Json.Formatting.None)} }}";
            using (var ccnet = new CCNET())
            {
                JObject project = JObject.Parse(json);
                var xmlConfig = ProjectWebJsonConverter.WebJObjectToSerializedProjectConverter.Convert(project);
                if (ccnet.Server.GetProjectStatus().Any(t => t.Name == id))
                {
                    ccnet.Server.DeleteProject(id, false, false, false);
                }

                ccnet.Server.AddProject(xmlConfig);
                ccnet.WaitAddComplete(id);
            }
        }

        public void Delete(string id)
        {
            using (var ccnet = new CCNET())
            {
                ccnet.Server.DeleteProject(id, false, true, true);
                ccnet.WaitDeleteComplete(id);
            }
        }
    }
}