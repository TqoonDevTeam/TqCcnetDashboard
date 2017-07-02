using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace TqLib.Utils
{
    public class ExecutableUtil
    {
        public string Executable { get; set; }
        public string Args { get; set; }
        public string WorkingDirectory { get; set; }
        public int Timeout { get; set; } = 120000;
        public int[] SuccessCodes { get; set; } = new[] { 0 };

        public ExecuteResult Run()
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            int exitCode = -1;
            try
            {
                using (var proc = new Process())
                {
                    proc.StartInfo = new ProcessStartInfo
                    {
                        FileName = Executable,
                        Arguments = Args,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    {
                        proc.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                if (!outputWaitHandle.SafeWaitHandle.IsClosed) outputWaitHandle.Set();
                            }
                            else output.AppendLine(e.Data);
                        };
                    }
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        proc.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                if (!errorWaitHandle.SafeWaitHandle.IsClosed) errorWaitHandle.Set();
                            }
                            else error.AppendLine(e.Data);
                        };
                    }

                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit(Timeout);
                    exitCode = proc.ExitCode;
                }
            }
            catch (Exception ex)
            {
                error.Append(ex);
            }
            return new ExecuteResult
            {
                ExitCode = exitCode,
                Output = output.ToString(),
                Error = error.ToString(),
                WasSuccess = SuccessCodes.Contains(exitCode)
            };
        }

        public class ExecuteResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
            public bool WasSuccess { get; set; }
        }
    }
}