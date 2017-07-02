using Microsoft.Win32;

namespace TqLib.ccnet.Local.Helper
{
    public class CcnetServiceFinder
    {
        public string ServiceName { get; private set; }

        public CcnetServiceFinder(string serviceName = "CCService")
        {
            ServiceName = serviceName;
        }

        public string FindServiceDirectory()
        {
            RegistryKey keyHKLM = Registry.LocalMachine;
            var key = keyHKLM.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{ServiceName}");
            return System.IO.Path.GetDirectoryName((key?.GetValue("ImagePath") ?? string.Empty).ToString());
        }
    }
}