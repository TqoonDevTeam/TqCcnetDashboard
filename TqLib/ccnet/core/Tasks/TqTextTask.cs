﻿using Exortech.NetReflector;
using System.IO;
using System.Text;
using System.Xml.Linq;
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

        [ReflectorProperty("saveType", Required = false)]
        public SaveType FileSaveType { get; set; } = SaveType.Text;

        [ReflectorProperty("saveCondition", Required = false)]
        public SaveCondition FileSaveCondition { get; set; } = SaveCondition.IfChanged;

        protected override bool Execute(IIntegrationResult result)
        {
            var savePath = GetSavePath(result);

            string saveData;
            if (FileSaveType == SaveType.Xml)
            {
                StringBuilder sb = new StringBuilder();
                using (TextWriter writer = new StringWriter(sb))
                {
                    XDocument.Parse(Source).Save(writer);
                }
                saveData = sb.ToString();
            }
            else
            {
                saveData = Source;
            }

            bool canSave = true;
            if (FileSaveCondition == SaveCondition.IfChanged)
            {
                if (File.Exists(savePath))
                {
                    var oldData = File.ReadAllText(savePath).Trim();
                    var newData = saveData.Trim();
                    canSave = !oldData.Equals(newData);
                }
            }

            if (canSave)
            {
                File.WriteAllText(savePath, saveData);
            }

            return true;
        }

        private string GetSavePath(IIntegrationResult result)
        {
            return result.BaseFromWorkingDirectory(SavePath);
        }

        public enum SaveType
        {
            Text, Xml
        }

        public enum SaveCondition
        {
            IfChanged, Force
        }
    }
}