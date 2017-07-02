using Exortech.NetReflector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqRsync", Description = "TqRsync")]
    public class TqRsyncTask : BaseExecutableTask
    {
        private readonly IExecutionEnvironment executionEnvironment;

        [ReflectorProperty("executable", Required = false)]
        public string Executable { get; set; }

        [ReflectorProperty("src")]
        public string Src { get; set; }

        [ReflectorProperty("dest")]
        public string Dest { get; set; }

        [ReflectorProperty("password", Required = false)]
        public string Password { get; set; }

        [ReflectorProperty("priority", Required = false)]
        public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.Normal;

        [ReflectorProperty("timeout", Required = false)]
        public int Timeout { get; set; } = 120;

        [ReflectorProperty("workingDirectory", Required = false)]
        public string WorkingDirectory { get; set; }

        [ReflectorProperty("options")]
        public string Options { get; set; }

        public TqRsyncTask() : this(new ProcessExecutor(), new ExecutionEnvironment(), new DefaultShadowCopier())
        {
        }

        public TqRsyncTask(ProcessExecutor executor, IExecutionEnvironment executionEnvironment, IShadowCopier shadowCopier)
        {
            this.executor = executor;
            this.executionEnvironment = executionEnvironment;
        }

        protected override bool Execute(IIntegrationResult result)
        {
            SetDefaultEnvironmentVariable();
            BuildProgressInformation buildProgressInformation = result.BuildProgressInformation;
            buildProgressInformation.SignalStartRunTask($"Executing TqRsync");

            ProcessInfo processInfo = this.CreateProcessInfo(result);

            result.AddTaskResult(processInfo.FileName + " " + processInfo.Arguments);

            ProcessResult run = base.TryToRun(processInfo, result);
            result.AddTaskResult(new ProcessTaskResult(run, true));
            if (run.TimedOut)
            {
                result.AddTaskResult(BaseExecutableTask.MakeTimeoutBuildResult(processInfo));
            }
            return run.Succeeded;
        }

        protected override string GetProcessArguments(IIntegrationResult result)
        {
            ProcessArgumentBuilder processArgumentBuilder = new ProcessArgumentBuilder();
            if (!string.IsNullOrEmpty(Options.Trim()))
            {
                processArgumentBuilder.Append(Options.Trim());
            }
            processArgumentBuilder.AddArgument(Src.Trim());
            processArgumentBuilder.AddArgument(Dest.Trim());
            return processArgumentBuilder.ToString();
        }

        protected override string GetProcessBaseDirectory(IIntegrationResult result)
        {
            return result.BaseFromWorkingDirectory(WorkingDirectory);
        }

        protected override string GetProcessFilename()
        {
            if (string.IsNullOrEmpty(Executable))
            {
                return @"C:\Program Files (x86)\ICW\Bin\rsync.exe";
            }
            else
            {
                return Executable;
            }
        }

        protected override ProcessPriorityClass GetProcessPriorityClass()
        {
            return Priority;
        }

        protected override int GetProcessTimeout()
        {
            return Timeout * 1000;
        }

        private void SetDefaultEnvironmentVariable()
        {
            var list = EnvironmentVariables?.ToList() ?? new List<EnvironmentVariable>();

            if (!string.IsNullOrEmpty(Password))
            {
                SetEnvironmentVariable(list, "RSYNC_PASSWORD", Password);
            }

            EnvironmentVariables = list.ToArray();
        }

        private void SetEnvironmentVariable(List<EnvironmentVariable> list, string name, string value)
        {
            var pw = list.FirstOrDefault(t => t.name == name);
            if (pw != null)
            {
                pw.value = value;
            }
            else
            {
                list.Add(new EnvironmentVariable { name = name, value = value });
            }
        }
    }
}