using ThoughtWorks.CruiseControl.Core;

namespace TqLib.ccnet.Core
{
    public static class Extension
    {
        public static void AddMessage(this IIntegrationResult result, string message)
        {
            result.AddTaskResult(message + System.Environment.NewLine);
        }

        public static void SetParameters(this IIntegrationResult result, string name, string value)
        {
            var index = result.Parameters.FindIndex(t => t.Name == name);
            if (index > -1)
            {
                result.Parameters[index].Value = value;
            }
            else
            {
                result.Parameters.Add(new ThoughtWorks.CruiseControl.Remote.NameValuePair(name, value));
            }
        }

        public static string GetParameters(this IIntegrationResult result, string name)
        {
            var index = result.Parameters.FindIndex(t => t.Name == name);
            if (index > -1)
            {
                return result.Parameters[index].Value;
            }
            else
            {
                return null;
            }
        }

        public static void SetSourceData(this IIntegrationResult result, string name, string value)
        {
            var index = result.SourceControlData.FindIndex(t => t.Name == name);
            if (index > -1)
            {
                result.SourceControlData[index].Value = value;
            }
            else
            {
                result.SourceControlData.Add(new ThoughtWorks.CruiseControl.Remote.NameValuePair(name, value));
            }
        }

        public static string GetSourceData(this IIntegrationResult result, string name)
        {
            var index = result.SourceControlData.FindIndex(t => t.Name == name);
            if (index > -1)
            {
                return result.SourceControlData[index].Value;
            }
            else
            {
                return null;
            }
        }
    }
}