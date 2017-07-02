using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using TqLib.ccnet.Local;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqProjectRemove")]
    public class TqProjectRemoveTask : TaskBase
    {
        [ReflectorProperty("projectName", Required = false)]
        public string ProjectName { get; set; }

        [ReflectorProperty("host", Required = false)]
        public string Host { get; set; } = "127.0.0.1";

        protected override bool Execute(IIntegrationResult result)
        {
            string projectName = GetProjectName(result);
            using (var ccnet = new CCNET(Host))
            {
                ccnet.Server.DeleteProject(projectName, false, true, true);
            }
            return true;
        }

        private string GetProjectName(IIntegrationResult result)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return result.ProjectName;
            }
            else
            {
                return ProjectName;
            }
        }
    }
}