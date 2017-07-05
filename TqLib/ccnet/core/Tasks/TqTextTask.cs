using Exortech.NetReflector;
using System.IO;
using System.Text;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqText", Description = "TqText")]
    public class TqTextTask : TaskBase
    {
        [ReflectorProperty("source")]
        public string Source { get; set; } = string.Empty;

        [ReflectorProperty("savePath")]
        public string SavePath { get; set; }

        [ReflectorProperty("saveEncoding", Required = false)]
        public string SaveEncoding { get; set; } = "UTF-8";

        [ReflectorProperty("saveCondition", Required = false)]
        public SaveCondition FileSaveCondition { get; set; } = SaveCondition.IfChanged;

        protected override bool Execute(IIntegrationResult result)
        {
            var savePath = GetSavePath(result);
            bool isChanged = true;
            var encoding = Encoding.GetEncoding(SaveEncoding);
            if (FileSaveCondition == SaveCondition.IfChanged)
            {
                if (File.Exists(savePath))
                {
                    isChanged = !Source.Equals(File.ReadAllText(savePath, encoding));
                }
            }

            if (isChanged)
            {
                File.WriteAllText(savePath, Source, encoding);
            }

            return true;
        }

        private string GetSavePath(IIntegrationResult result)
        {
            return result.BaseFromWorkingDirectory(SavePath);
        }

        public enum SaveCondition
        {
            IfChanged, Force
        }
    }
}