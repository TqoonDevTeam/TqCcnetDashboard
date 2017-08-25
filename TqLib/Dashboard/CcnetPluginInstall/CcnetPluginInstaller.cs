using System.IO;

namespace TqLib.Dashboard.CcnetPluginInstall
{
    public class CcnetPluginInstaller
    {
        public string[] DisAllowFIleNames { get; set; } = new string[] { };
        private string SrcDirectory, PluginDirectory, ServiceDirectory, ServicePluginReferenceDirectory;

        /// <summary>
        /// CcnetPluginInstaller
        /// </summary>
        /// <param name="srcDirectory">플러그인소스디렉토리</param>
        /// <param name="pluginDirectory">플러그인디렉토리</param>
        /// <param name="serviceDirecotry">CCNET서비스디렉토리</param>
        public CcnetPluginInstaller(string srcDirectory, string pluginDirectory, string serviceDirecotry)
        {
            SrcDirectory = srcDirectory;
            PluginDirectory = pluginDirectory;
            ServiceDirectory = serviceDirecotry;
            ServicePluginReferenceDirectory = Path.Combine(ServiceDirectory, "$$PluginReference");
        }

        /// <summary>
        /// 설치
        /// </summary>
        public void Install()
        {
            // init
            CheckAndCreatePluginFolder();
            CcnetPluginInstallPackage package = GetCcnetPluginInstallPackage();

            // Install
            InstallPluginFiles(package);
            InstallPluginReferenceFiles(package);

            // assemblyBinding
            CcnetServicePluginRefrenceUpdator ccnetServicePluginRefrenceUpdator = new CcnetServicePluginRefrenceUpdator(PluginDirectory, ServiceDirectory, ServicePluginReferenceDirectory);
            ccnetServicePluginRefrenceUpdator.Update();
        }

        private string GetPluginCopyPath(string srcPluginPath)
        {
            return Path.Combine(PluginDirectory, Path.GetFileName(srcPluginPath));
        }

        private string GetPluginReferenceCopyPath(string srcPluginPath, string srcPluginReferencePath)
        {
            return Path.Combine(GetPluginReferenceDirectory(srcPluginPath), Path.GetFileName(srcPluginReferencePath));
        }

        private string GetPluginReferenceDirectory(string srcPluginPath)
        {
            return Path.Combine(PluginDirectory, Path.GetFileNameWithoutExtension(srcPluginPath));
        }

        private CcnetPluginInstallPackage GetCcnetPluginInstallPackage()
        {
            return new CcnetPluginInstallPackage(SrcDirectory);
        }

        private void InstallPluginFiles(CcnetPluginInstallPackage package)
        {
            string copyPath;
            foreach (var file in package.PluginFiles)
            {
                copyPath = GetPluginCopyPath(file);
                File.Copy(file, copyPath, true);
            }
        }

        private void InstallPluginReferenceFiles(CcnetPluginInstallPackage package)
        {
            string pluginReferenceDirectory, pluginReferenceCopyPath;
            foreach (var srcPluginPath in package.PluginFiles)
            {
                pluginReferenceDirectory = GetPluginReferenceDirectory(srcPluginPath);
                CheckAndCreateDirectory(pluginReferenceDirectory, true);

                foreach (var srcPluginReferencePath in package.PluginReferenceFiles)
                {
                    pluginReferenceCopyPath = GetPluginReferenceCopyPath(srcPluginPath, srcPluginReferencePath);
                    File.Copy(srcPluginReferencePath, pluginReferenceCopyPath);
                }
            }
        }

        private void CheckAndCreatePluginFolder()
        {
            CheckAndCreateDirectory(PluginDirectory);
        }

        private void CheckAndCreateDirectory(string path, bool delete = false)
        {
            if (delete)
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}