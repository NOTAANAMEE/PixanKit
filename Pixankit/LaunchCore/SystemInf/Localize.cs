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

        public static string CPLocalize(string cparg)
        {
            return SysInfo.OSName switch
            {
                "windows" => cparg.Replace("${classpath_separator}", ";"),
                _ => cparg,
            };
        }

        public static string LocalParser = "${classpath_separator}";
    }
}
