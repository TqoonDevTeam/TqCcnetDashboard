using Exortech.NetReflector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using TqLib.ccnet.Core.Util;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqNunit", Description = "Nunit")]
    public class TqNunitTask : BaseExecutableTask
    {
        private static string Executable_V2 = string.Empty;
        private static string Executable_V3 = string.Empty;
        private readonly IExecutionEnvironment executionEnvironment;

        [ReflectorProperty("assemblies", Description = "라이브러리 파일명")]
        public string[] Assemblies { get; set; }

        [ReflectorProperty("version")]
        public string Version { get; set; } = "3.0";

        [ReflectorProperty("excludedCategories", Required = false)]
        public string[] ExcludedCategories { get; set; }

        [ReflectorProperty("includedCategories", Required = false)]
        public string[] IncludedCategories { get; set; }

        [ReflectorProperty("options", typeof(StringDictionarySerializerFactory), Required = false)]
        public Dictionary<string, string> Options { get; set; }

        [ReflectorProperty("priority", Required = false)]
        public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.Normal;

        [ReflectorProperty("timeout", Required = false)]
        public int Timeout { get; set; } = 600;

        [ReflectorProperty("workingDirectory", Description = "빌드 된 라이브러리가 존재하는 위치 입니다.")]
        public string WorkingDirectory { get; set; }

        public TqNunitTask() : this(new ProcessExecutor(), new ExecutionEnvironment(), new DefaultShadowCopier())
        {
        }

        public TqNunitTask(ProcessExecutor executor, IExecutionEnvironment executionEnvironment, IShadowCopier shadowCopier)
        {
            this.executor = executor;
            this.executionEnvironment = executionEnvironment;
        }

        protected override bool Execute(IIntegrationResult result)
        {
            BuildProgressInformation buildProgressInformation = result.BuildProgressInformation;
            buildProgressInformation.SignalStartRunTask($"Executing Nunit{Version}");
            ProcessInfo processInfo = this.CreateProcessInfo(result);
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
            if (Version.StartsWith("3"))
            {
                if (!Options.ContainsKey("skipnontestassemblies"))
                {
                    Options["skipnontestassemblies"] = string.Empty;
                }

                Options["noresult"] = string.Empty;

                return new NUnitArgument3(GetDefaultAssemblies(), "", Options)
                {
                    ExcludedCategories = ExcludedCategories,
                    IncludedCategories = IncludedCategories
                }.ToString();
            }
            else
            {
                return new NUnitArgument(GetDefaultAssemblies(), "")
                {
                    ExcludedCategories = ExcludedCategories,
                    IncludedCategories = IncludedCategories
                }.ToString();
            }
        }

        protected override string GetProcessBaseDirectory(IIntegrationResult result)
        {
            return result.BaseFromWorkingDirectory(WorkingDirectory);
        }

        protected override string GetProcessFilename()
        {
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
            if (Version.StartsWith("3"))
            {
                if (string.IsNullOrEmpty(Executable_V3))
                {
                    Executable_V3 = @"C:\Program Files (x86)\NUnit.org\nunit-console\nunit3-console.exe";
                }
                return Executable_V3;
            }
            else
            {
                if (string.IsNullOrEmpty(Executable_V2))
                {
                    var dir = Directory.GetDirectories(@"C:\Program Files (x86)", "NUnit 2*");
                    string[] files;
                    foreach (var d in dir)
                    {
                        files = Directory.GetFiles(d, "nunit-console.exe", SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            Executable_V2 = files[0];
                            break;
                        }
                    }
                }
                return Executable_V2;
            }
        }

        private string[] GetDefaultAssemblies()
        {
            return Assemblies;
        }

        public class NUnitArgument3 : NUnitArgument
        {
            private string outputfile = string.Empty;
            private Dictionary<string, string> Options;

            public NUnitArgument3(string[] assemblies, string outputfile, Dictionary<string, string> options = null) : base(assemblies, outputfile)
            {
                this.Options = options;
                this.outputfile = outputfile;
            }

            private void AppendCategoriesArg(ProcessArgumentBuilder argsBuilder)
            {
                string arg = string.Empty;

                if (this.ExcludedCategories != null && (int)this.ExcludedCategories.Length != 0)
                {
                    string[] strArrays = Array.FindAll<string>(this.ExcludedCategories, new Predicate<string>(NUnitArgument3.IsNotWhitespace));
                    var cat = string.Join(" && ", strArrays.Select(t => $"cat != {t}"));
                    arg = $"({cat})";
                }

                if (this.IncludedCategories != null && (int)this.IncludedCategories.Length != 0)
                {
                    string[] strArrays1 = Array.FindAll<string>(this.IncludedCategories, new Predicate<string>(NUnitArgument3.IsNotWhitespace));
                    var cat1 = string.Join(" || ", strArrays1.Select(t => $"cat == {t}"));
                    arg += $"{(string.IsNullOrEmpty(arg) ? string.Empty : " && ")}({cat1})";
                }

                if (!string.IsNullOrEmpty(arg)) argsBuilder.AddArgument("--where", "=", arg);
            }

            private static bool IsNotWhitespace(string input)
            {
                return !StringUtil.IsWhitespace(input);
            }

            public override string ToString()
            {
                ProcessArgumentBuilder processArgumentBuilder = new ProcessArgumentBuilder();
                string[] strArrays = this.assemblies;
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    processArgumentBuilder.AddArgument(strArrays[i]);
                }
                if (!string.IsNullOrEmpty(outputfile)) processArgumentBuilder.AddArgument("--output", "=", outputfile);
                this.AppendCategoriesArg(processArgumentBuilder);

                if (Options != null)
                {
                    foreach (var kv in Options)
                    {
                        if (string.IsNullOrEmpty(kv.Value))
                        {
                            processArgumentBuilder.AddArgument("--" + kv.Key);
                        }
                        else
                        {
                            processArgumentBuilder.AddArgument("--" + kv.Key, "=", kv.Value);
                        }
                    }
                }

                Log.Info("ProcessArgumentBuilder : " + processArgumentBuilder.ToString());
                return processArgumentBuilder.ToString();
            }
        }
    }
}