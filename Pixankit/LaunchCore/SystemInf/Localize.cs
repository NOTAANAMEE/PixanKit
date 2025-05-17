namespace PixanKit.LaunchCore.SystemInf;

internal static class Localize
{
    private static string _userPath;

    static Localize()
    {
        _userPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        if (SysInfo.OsName == "windows")
            _userPath = _userPath[0.._userPath.LastIndexOf('\\')];
    }

    public static string CpLocalize(string cparg)
    {
        return SysInfo.OsName switch
        {
            "windows" => cparg.Replace("${classpath_separator}", ";"),
            _ => cparg,
        };
    }

    public static string LocalParser = "${classpath_separator}";
}