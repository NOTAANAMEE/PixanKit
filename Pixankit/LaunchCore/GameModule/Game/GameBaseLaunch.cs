using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.GameModule.Game;

public abstract partial class GameBase
{
    #region LaunchMethods
    public partial string GetLaunchArgument()
    {
        var arg = GetArgument();
        arg = arg.Replace("${natives_directory}", $"\"{NativeDirPath}\"");
        arg = arg.Replace("${assets_root}", $"\"{AssetsDirPath}\"");
        arg = arg.Replace("${assets_index_name}", GetAssetsId());
        arg = arg.Replace("${classpath}", "\"" + GetCpArgs() + "\"");
        arg = arg.Replace("${version_name}", Version);
        arg = arg.Replace("${version_type}", ReleaseType);
        arg = arg.Replace("${library_directory}", LibrariesDirPath);
        arg = Localize.CpLocalize(arg);
        Logger.Logger.Info($"Arguments Generated. Targeted Game:{GameFolderPath}");
        return "${jvm_argument} " + arg;
    }
        
    protected virtual partial string GetArgument()
        => Params.JavaArgs + $" {Params.MainClass} " + Params.GameArgs;
        
    protected virtual partial string GetAssetsId()
        => Params.AssetsId;
        
    public virtual partial bool LaunchCheck() => true;
        
    protected virtual partial string GetCpArgs()
    {
        var classpath = LibrariesRef.Libraries.Aggregate("", (current, library) => current + (library.LibraryPath + Localize.LocalParser));
        classpath += GameJarFilePath;
        return classpath;
    }

    internal static string GetCpArgs(IEnumerable<LibraryBase> libs)
    {
        return libs.Aggregate("", (current, library) => current + (library.LibraryPath + Localize.LocalParser));
    }
        
    public partial async Task Decompress()
    {
        await LibrariesRef.Extract(this.LibrariesDirPath, this.NativeDirPath);
    }
    #endregion
}