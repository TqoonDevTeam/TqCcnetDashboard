using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TqLib.Dashboard.CcnetPluginInstall
{
    public class CcnetServicePluginRefrenceUpdator
    {
        public string PluginDirectory { get; private set; }
        public string ServiceDirectory { get; private set; }
        public string ServicePluginReferenceDirectory { get; private set; }

        public CcnetServicePluginRefrenceUpdator(string pluginDirectory, string serviceDirectory, string servicePluginReferenceDirectory)
        {
            PluginDirectory = pluginDirectory;
            ServiceDirectory = serviceDirectory;
            ServicePluginReferenceDirectory = servicePluginReferenceDirectory;
            if (!Directory.Exists(ServicePluginReferenceDirectory)) Directory.CreateDirectory(ServicePluginReferenceDirectory);
        }

        public void Update()
        {
            var allPluginRefrenceInfo = GetAllPluginRefrenceInfo();
            Update_PluginRefrenceDirectory(allPluginRefrenceInfo);
            Update_ServiceConfigAssemblyBinding(allPluginRefrenceInfo);
        }

        private void Update_ServiceConfigAssemblyBinding(IList<PluginRefrenceInfo> allPluginRefrenceInfo)
        {
            foreach (var configPath in GetServiceConfigPath())
            {
                AssemblyBinding assemblyBinding = new AssemblyBinding();
                assemblyBinding.Load(configPath);

                // probing 으로 정상 작동함으로 상세 구성은 일단 제외한다.
                assemblyBinding.Clear();
                foreach (var item in allPluginRefrenceInfo.Where(t => t.IsDll()))
                {
                    assemblyBinding.Append(new DependentAssembly
                    {
                        assemblyIdentity = new AssemblyIdentity
                        {
                            culture = item.culture,
                            name = item.asmName,
                            publicKeyToken = item.publicKeyToken
                        },
                        bindingRedirect = new BindingRedirect
                        {
                            oldVersion = "0.0.0.0-99.0.0.0",
                            newVersion = item.version
                        },
                        codeBase = new CodeBase
                        {
                            version = item.version,
                            href = GetInstalledPluginReferenceTargetPath(item.name)
                        }
                    });
                }
                assemblyBinding.Save();
            }
        }

        private string[] GetServiceConfigPath()
        {
            var debugConfig = Path.Combine(ServiceDirectory, "ccnet.exe.config");
            var serviceConfig = Path.Combine(ServiceDirectory, "ccservice.exe.config");
            return new[] { debugConfig, serviceConfig };
        }

        private void Update_PluginRefrenceDirectory(IList<PluginRefrenceInfo> allPluginRefrenceInfo)
        {
            PluginRefrenceInfo installedPluginReference;
            foreach (var item in allPluginRefrenceInfo)
            {
                installedPluginReference = FindInstalledPluginReference(item.name);
                if (installedPluginReference == null || item.CompareTo(installedPluginReference) >= 0)
                {
                    File.Copy(item.path, GetInstalledPluginReferenceTargetPath(item.name), true);
                }
            }

            // 불필요 파일 삭제
            var installed = GetAllInstalledPluginRefrenceInfo();
            var deleteTarget = installed.Where(t => !allPluginRefrenceInfo.Any(t2 => t2.Equals(t)));
            foreach (var file in deleteTarget)
            {
                File.Delete(file.path);
            }
        }

        private string GetInstalledPluginReferenceTargetPath(string srcFileName)
        {
            return Path.Combine(ServicePluginReferenceDirectory, srcFileName);
        }

        private PluginRefrenceInfo FindInstalledPluginReference(string fileName)
        {
            var installedPluginReferencePath = Path.Combine(ServicePluginReferenceDirectory, fileName);
            if (File.Exists(installedPluginReferencePath))
            {
                return new PluginRefrenceInfo(installedPluginReferencePath);
            }
            return null;
        }

        private IList<PluginRefrenceInfo> GetAllPluginRefrenceInfo()
        {
            IList<PluginRefrenceInfo> result = new List<PluginRefrenceInfo>();
            PluginRefrenceInfo oldItem, newItem;
            foreach (var srcPluginDirectory in new DirectoryInfo(PluginDirectory).GetDirectories())
            {
                foreach (var srcPluginReferencePath in srcPluginDirectory.GetFiles("*.*"))
                {
                    newItem = new PluginRefrenceInfo(srcPluginReferencePath.FullName);
                    oldItem = result.FirstOrDefault(t => t.Equals(newItem));

                    if (oldItem == null)
                    {
                        result.Add(newItem);
                    }
                    else
                    {
                        if (oldItem.CompareTo(newItem) <= 0)
                        {
                            result.Remove(oldItem);
                            result.Add(newItem);
                        }
                    }
                }
            }
            return result;
        }

        private IList<PluginRefrenceInfo> GetAllInstalledPluginRefrenceInfo()
        {
            IList<PluginRefrenceInfo> result = new List<PluginRefrenceInfo>();
            foreach (var file in Directory.GetFiles(ServicePluginReferenceDirectory, "*.*"))
            {
                result.Add(new PluginRefrenceInfo(file));
            }
            return result;
        }

        private class PluginRefrenceInfo : IComparable
        {
            public string name { get; set; }
            public string path { get; set; }
            public string asmName { get; set; }
            public string version { get; set; }
            public string publicKeyToken { get; set; }
            public string culture { get; set; }

            public PluginRefrenceInfo(string srcPluginReferencePath)
            {
                name = Path.GetFileName(srcPluginReferencePath);
                path = srcPluginReferencePath;

                if (IsVersionValidRefrence(srcPluginReferencePath))
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(srcPluginReferencePath);
                    asmName = assemblyName.Name;
                    version = assemblyName.Version.ToString();
                    publicKeyToken = assemblyName.GetPublicKeyToken().ConvertToHex();
                    culture = string.IsNullOrEmpty(assemblyName.CultureName) ? "neutral" : assemblyName.CultureName;
                }
                else
                {
                    asmName = name;
                    version = "0.0.0.0";
                    publicKeyToken = "";
                    culture = "neutral";
                }
            }

            private bool IsVersionValidRefrence(string srcPluginReferencePath)
            {
                return new[] { ".dll" }.Any(t => t.Equals(Path.GetExtension(srcPluginReferencePath)));
            }

            public override bool Equals(object obj)
            {
                PluginRefrenceInfo obj2 = obj as PluginRefrenceInfo;
                return name.Equals(obj2.name, StringComparison.OrdinalIgnoreCase) && publicKeyToken.Equals(obj2.publicKeyToken);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public int CompareTo(object obj)
            {
                return version.CompareTo((obj as PluginRefrenceInfo).version);
            }

            public bool IsDll()
            {
                return name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}