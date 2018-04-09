using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TqLib.Dashboard.CcnetPluginInstall
{
    public class CcnetPluginInstallPackage
    {
        private string PluginSourceDirectory;

        public string[] AllowExtensions { get; set; } = new[] { ".dll", ".exe" };
        public IList<string> PluginFiles { get; private set; }
        public IList<string> PluginReferenceFiles { get; private set; }
        public IList<string> AllFiles { get; private set; }

        public CcnetPluginInstallPackage(string pluginSourceDirectory)
        {
            PluginSourceDirectory = pluginSourceDirectory;
            AllFiles = GetAllFiles();
            PluginFiles = GetPluginFiles();
            PluginReferenceFiles = GetPluginReferenceFiles();
        }

        private IList<string> GetAllFiles()
        {
            return Directory.GetFiles(PluginSourceDirectory, "*.*", SearchOption.AllDirectories).Where(file => AllowExtensions.Any(extension => extension.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))).ToList();
        }

        private IList<string> GetPluginFiles()
        {
            return AllFiles?.Where(file =>
            {
                if (".dll".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                {
                    string assemblyName = AssemblyName.GetAssemblyName(file).Name;
                    if (assemblyName.StartsWith("ccnet.") && assemblyName.EndsWith(".plugin"))
                    {
                        return true;
                    }
                }
                return false;
            }).ToList() ?? new List<string> { };
        }

        private IList<string> GetPluginReferenceFiles()
        {
            return AllFiles.Except(PluginFiles).ToList();
        }
    }
}