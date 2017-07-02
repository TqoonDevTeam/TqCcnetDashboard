using Exortech.NetReflector;
using System.Diagnostics;
using System.IO;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using TqLib.Utils;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("nuget")]
    public class NugetTask : BaseExecutableTask
    {
        private readonly IExecutionEnvironment executionEnvironment;

        [ReflectorProperty("executable", Required = false)]
        public string Executable { get; set; } = "nuget.exe";

        [ReflectorProperty("timeout", Required = false)]
        public int Timeout { get; set; } = 600;

        [ReflectorProperty("workingDirectory", Required = false)]
        public string WorkingDirectory { get; set; }

        [ReflectorProperty("buildArgs", Required = false)]
        public string BuildArgs { get; set; } = "restore";

        [ReflectorProperty("priority", Required = false)]
        public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.Normal;

        [ReflectorProperty("projectFile")]
        public string ProjectFile { get; set; }

        public NugetTask() : this(new ProcessExecutor(), new ExecutionEnvironment(), new DefaultShadowCopier())
        {
        }

        public NugetTask(ProcessExecutor executor, IExecutionEnvironment executionEnvironment, IShadowCopier shadowCopier)
        {
            this.executor = executor;
            this.executionEnvironment = executionEnvironment;
            Executable = this.GetDefaultExecutable();
        }

        protected override bool Execute(IIntegrationResult result)
        {
            ProcessInfo processInfo = this.CreateProcessInfo(result);
            ProcessResult run = base.TryToRun(processInfo, result);
            result.AddTaskResult(run.StandardOutput);
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
            processArgumentBuilder.AppendArgument(BuildArgs);
            string projectFile = GetProjectFile(result);
            if (!string.IsNullOrEmpty(projectFile)) processArgumentBuilder.AddArgument(projectFile);
            return processArgumentBuilder.ToString();
        }

        protected override string GetProcessBaseDirectory(IIntegrationResult result)
        {
            return result.BaseFromWorkingDirectory(WorkingDirectory);
        }

        protected override string GetProcessFilename()
        {
            if (!string.IsNullOrEmpty(Executable))
            {
                return Executable;
            }
            return GetDefaultExecutable();
        }

        protected override ProcessPriorityClass GetProcessPriorityClass()
        {
            return Priority;
        }

        protected override int GetProcessTimeout()
        {
            return Timeout * 1000;
        }

        private string GetDefaultExecutable()
        {
            return Executable;
        }

        private string GetProjectFile(IIntegrationResult result)
        {
            if (string.IsNullOrEmpty(ProjectFile)) return string.Empty;
            var workindDirectory = GetProcessBaseDirectory(result);
            return PathUtil.Combine(workindDirectory, ProjectFile);
        }
    }
}