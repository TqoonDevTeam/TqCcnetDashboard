using ThoughtWorks.CruiseControl.Core;

namespace TqLib.ccnet.Core
{
    public static class Extension
    {
        public static void AddMessage(this IIntegrationResult result, string message)
        {
            result.AddTaskResult(System.Environment.NewLine + message);
        }
    }
}