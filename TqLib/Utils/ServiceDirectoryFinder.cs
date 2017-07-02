using Microsoft.Win32;

namespace TqLib.Utils
{
    public class ServiceDirectoryFinder
    {
        public string FindServiceDirectory(string serviceName)
        {
            RegistryKey keyHKLM = Registry.LocalMachine;
            var key = keyHKLM.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
            return System.IO.Path.GetDirectoryName((key?.GetValue("ImagePath") ?? string.Empty).ToString());
        }
    }
}