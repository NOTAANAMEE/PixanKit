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
        var classpath = 
            string.Join("${classpath_separator}", 
                GetLibraries().Select(a => a.LibraryPath));
        classpath += "${classpath_separator}" + GameJarFilePath;
        return classpath;
    }
        
    //public partial async Task Decompress()
    //{
        //await LibrariesRef.Extract(this.LibrariesDirPath, this.NativeDirPath);
    //}
    #endregion
}