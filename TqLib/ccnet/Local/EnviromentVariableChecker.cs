using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TqLib.ccnet.Local
{
    public class EnviromentVariableChecker
    {
        public string[] Target { get; private set; }
        public IDictionary<string, bool> Result { get; private set; } = new Dictionary<string, bool>();

        public EnviromentVariableChecker() : this(new[] { "svn", "git", "msbuild", "nuget", "rsync" })
        {
        }

        public EnviromentVariableChecker(string[] target)
        {
            Target = target;
        }

        public IDictionary<string, bool> Check()
        {
            foreach (var k in Target)
            {
                Result[k] = ExistsExec(k);
            }
            return Result;
        }

        private bool ExistsExec(string exe)
        {
            ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = "where.exe";
            cmdStartInfo.Arguments = exe;
            cmdStartInfo.UseShellExecute = false;
            using (var cmd = new Process())
            {
                cmd.StartInfo = cmdStartInfo;
                cmd.Start();
                cmd.WaitForExit();
                return cmd.ExitCode == 0;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}