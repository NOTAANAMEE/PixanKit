using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.ResourceDownloader;

internal static class Localize
{
    private static string _userPath;

    static Localize()
    {
        _userPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        if (SysInfo.OsName == "windows")
            _userPath = _userPath[0.._userPath.LastIndexOf('\\')];
    }

    public static string PathLocalize(string path)
    {
        if (SysInfo.OsName == "windows") path = path.Replace("~", _userPath);
        return SysInfo.OsName switch
        {
            "windows" => path.Replace("/", "\\"),
            _ => path,
        };
    }

    public static string GetLocalDirectory(string path)
    {
        return PathLocalize(path.Remove(path.LastIndexOf('/')));
    }

    public static string LocalParser
    {
        get
        {
            if (SysInfo.OsName == "windows") return ";";
            else return ":";
        }
    }

    public static string DeLocalize(string path)
    {
        return path.Replace('\\', '/');
    }

    public static void CheckDir(string path)
    {
        var dir = Path.GetDirectoryName(path) ?? "./";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }
}