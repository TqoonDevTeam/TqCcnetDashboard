using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;
using ThoughtWorks.CruiseControl.Core.Util;

namespace TqLib.ccnet.Core.Tasks.Conditions
{
    [ReflectorType("folderNotExistsCondition")]
    public class FolderNotExistsTaskCondition : ConditionBase
    {
        public IFileSystem FileSystem { get; set; }

        [ReflectorProperty("folder", Required = true)]
        public string FolderName { get; set; }

        protected override bool Evaluate(IIntegrationResult result)
        {
            string str = result.BaseFromWorkingDirectory(this.FolderName);
            LogDescriptionOrMessage(string.Concat("Checking for folder '", str, "'"));
            if (FileSystem == null)
            {
                FileSystem = new SystemIoFileSystem();
            }
            return !FileSystem.DirectoryExists(str);
        }
    }
}