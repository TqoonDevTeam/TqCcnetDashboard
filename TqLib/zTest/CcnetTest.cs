using NUnit.Framework;
using System.IO;
using System.Reflection;
using TqLib.Utils;

namespace TqLib.zTest
{
    public class CcnetTest : AbstractTest
    {
        [Test]
        public void Exists()
        {
            var ccc = PathUtil.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "MSBUILD.exe");

            //var a = @"C:\Program Files (x86)\CruiseControl.NET\plugin\ccnet.TqoonDevTeamLib.plugin.dll";
            //var assembly = Assembly.LoadFrom(a);
            //var b = assembly.GetType("NugetTask");


            //AppDomain d = AppDomain.CreateDomain("test", new Evidence(), new AppDomainSetup
            //{
            //    ApplicationBase = @"C:\Program Files (x86)\CruiseControl.NET\server",
            //    PrivateBinPath = @"C:\Program Files (x86)\CruiseControl.NET\plugin"
            //});

            //var b = d.GetAssemblies();
            //var aa = Assembly.LoadFrom(a, );
            //var b = aa.DefinedTypes;
            //var c = aa.ExportedTypes;
            //var aaa = aa.GetExportedTypes();
            //var types = typeof(TaskBase).Assembly.GetTypes().Where(t => t.IsDefined(typeof(ReflectorTypeAttribute), false));

            //RegistryKey keyHKLM = Registry.LocalMachine;
            //var key = keyHKLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\CCService");
            //var path = key.GetValue("ImagePath").ToString();
            //var dir = Path.GetDirectoryName(path);
            //var config = Path.Combine(dir, "ccservice.exe.config");
            //var doc = XDocument.Load(config);
            //var pluginAbs = doc.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(t => t.Attribute("key").Value == "PluginLocation").Attribute("value").Value;
            //var pluginPath = new Uri(new Uri(dir + "\\"), pluginAbs).LocalPath;

            //var dlls = new DirectoryInfo(pluginPath).GetFiles("*.dll").Select(t => t.FullName).ToArray();

            //foreach (var dll in dlls)
            //{
            //    types = types.Union(Assembly.LoadFile(dll).GetTypes().Where(t => t.IsDefined(typeof(ReflectorTypeAttribute), false)).ToArray());
            //}

            //var a = types.Distinct().ToArray();
        }
    }
}