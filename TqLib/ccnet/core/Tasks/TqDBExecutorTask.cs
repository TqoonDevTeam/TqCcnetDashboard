using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using TqLib.ccnet.Core.Util;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqDBExecutor", Description = "TqDBExecutorTask")]
    public class TqDBExecutorTask : TaskBase
    {
        [ReflectorProperty("provider")]
        public string Provider { get; set; }

        [ReflectorProperty("connectionString")]
        public string ConnectionString { get; set; }

        [ReflectorProperty("query")]
        public string Query { get; set; }

        protected override bool Execute(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask("Executing TqDBExecutor");

            var ado = SpringContextHelper.GetAdoTemplate(Provider, ConnectionString);
            ado.ExecuteNonQuery(System.Data.CommandType.Text, Query);

            return true;
        }
    }
}