using Microsoft.Win32;

namespace TqLib.ccnet.Utils
{
    public class CcnetServiceLocationFinder
    {
        public string ServiceName { get; private set; }

        public CcnetServiceLocationFinder(string serviceName = "CCService")
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