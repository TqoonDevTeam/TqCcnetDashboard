using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqLib.Utils;

namespace TqLib.zTest.Utils
{
    public class ExecuteableUtilTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var dir = @"C:\Program Files (x86)\CruiseControl.NET\tqdashboardUpdate\web";
            var DashboardFolder = @"C:\Program Files (x86)\CruiseControl.NET\tqoondashboard";
            var exec = new ExecutableUtil() { Executable = "robocopy.exe", SuccessCodes = new[] { 0, 1 } };
            exec.Args = $"\"{dir}\" \"{DashboardFolder}\" /E /PURGE /XA:H /XD Config";
            var result = exec.Run();
        }
    }
}
