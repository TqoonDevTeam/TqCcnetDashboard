using System.IO;
using System.Linq;

namespace TqLib.Utils
{
    public static class PathUtil
    {
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths.Select(t => CleanPath(t)).Where(t => !string.IsNullOrEmpty(t)).ToArray());
        }

        public static string CleanPath(string path, bool isWeb = false)
        {
            string SeparatorFrom, SeparatorTo;
            if (isWeb)
            {
                SeparatorFrom = @"\";
                SeparatorTo = "/";
            }
            else
            {
                SeparatorFrom = "/";
                SeparatorTo = @"\";
            }

            path = path.Replace(SeparatorFrom, SeparatorTo);

            if (path.StartsWith(SeparatorTo))
            {
                if (path == SeparatorTo)
                {
                    path = "";
                }
                else
                {
                    path = path.Substring(1);
                }
            }
            return path;
        }
    }
}