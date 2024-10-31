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
            if (SystemInformation.OSName == "windows") 
                UserPath = UserPath[0..UserPath.LastIndexOf('\\')];
        }

        public static string PathLocalize(string path)
        {
            if (SystemInformation.OSName == "windows") path = path.Replace("~", UserPath);
            switch (SystemInformation.OSName)
            {
                case "windows":
                    return path.Replace("/", "\\");
                default:
                    return path;
            }
        }

        public static string CPLocalize(string cparg)
        {
            switch (SystemInformation.OSName) 
            {
                case "windows":
                    return PathLocalize(cparg).Replace(":", ";");
                default:
                    return cparg;
            }
        }

        public static string LocalParser
        {
            get
            {
                if (SystemInformation.OSName == "windows") return ";";
                else return ":";
            }
        }

        public static string DeLocalize(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
