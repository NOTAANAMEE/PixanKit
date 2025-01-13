using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.SystemInf
{
    internal static class Localize
    {
        private static string UserPath;

        static Localize()
        {
            UserPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (SysInfo.OSName == "windows") 
                UserPath = UserPath[0..UserPath.LastIndexOf('\\')];
        }

        /*public static string PathLocalize(string path)
        {
            if (SysInfo.OSName == "windows") path = path.Replace("~", UserPath);
            switch (SysInfo.OSName)
            {
                case "windows":
                    return path.Replace("/", "\\");
                default:
                    return path;
            }
        }*/

        public static string CPLocalize(string cparg)
        {
            switch (SysInfo.OSName) 
            {
                case "windows":
                    return cparg.Replace("${classpath_separator}", ";");
                default:
                    return cparg;
            }
        }

        public static string LocalParser = "${classpath_separator}";

        /*public static string DeLocalize(string path)
        {
            return path.Replace('\\', '/');
        }*/
    }
}
