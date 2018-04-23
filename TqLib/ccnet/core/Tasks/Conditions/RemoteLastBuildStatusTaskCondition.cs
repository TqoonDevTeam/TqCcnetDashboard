using Exortech.NetReflector;
using System;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;
using ThoughtWorks.CruiseControl.Remote;

namespace TqLib.ccnet.Core.Tasks.Conditions
{
    [ReflectorType("remoteLastStatusCondition")]
    public class RemoteLastBuildStatusTaskCondition : ConditionBase
    {
        [ReflectorProperty("value", Required = true)]
        public IntegrationStatus Status { get; set; } = IntegrationStatus.Success;

        [ReflectorProperty("evaluation", Required = true)]
        public CompareValuesTaskCondition.Evaluation EvaluationType { get; set; }

        [ReflectorProperty("serverUri", Required = false)]
        public string ServerUri { get; set; } = "tcp://localhost:21234/CruiseManager.rem";

        [ReflectorProperty("projectName", Required = true)]
        public string ProjectName { get; set; }

        private readonly ICruiseServerClientFactory factory = new CruiseServerClientFactory();

        protected override bool Evaluate(IIntegrationResult result)
        {
            LogDescriptionOrMessage($"Checking status comparison condition - {ServerUri}");

            ProjectStatus project = GetProjectStatus();

            IntegrationStatus buildStatus = project?.BuildStatus ?? IntegrationStatus.Unknown;

            switch (EvaluationType)
            {
                case CompareValuesTaskCondition.Evaluation.Equal:
                    return Status == buildStatus;

                case CompareValuesTaskCondition.Evaluation.NotEqual:
                    return Status != buildStatus;

                default:
                    throw new InvalidOperationException("Unhandled evaluation type");
            }
        }

        private ProjectStatus GetProjectStatus()
        {
            using (var client = factory.GenerateClient(ServerUri))
            {
                return client.GetProjectStatus().FirstOrDefault(t => t.Name == ProjectName);
            }
        }
    }
}