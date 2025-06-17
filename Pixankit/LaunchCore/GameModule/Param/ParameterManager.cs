using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.Library;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.GameModule.Param;

/// <summary>
/// 
/// </summary>
public class ParameterManager
{
    private record GameParam(GameParameter parameter,
        LibraryCollection collection);

    private readonly Dictionary<string, GameParam> dict = [];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    /// <param name="param"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
    public void AddParameter(string version, GameParameter param, LibraryCollection collection)
    {
        dict[version] = new(param, collection);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public GameParameter GetParameter(GameBase game)
        => GetParameter(game.Version);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameVersion"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public GameParameter GetParameter(string gameVersion)
    {
        if (dict.TryGetValue(gameVersion, out var param))
            return param.parameter;
        throw new Exception("Parameter not found for the game.");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public LibraryCollection GetLibrary(GameBase game)
        => GetLibrary(game.Version);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameVersion"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public LibraryCollection GetLibrary(string gameVersion)
    {
        if (dict.TryGetValue(gameVersion, out var param))
            return param.collection;
        throw new Exception("Library not found for the game.");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    public string GenerateGameArg(GameBase game)
    {
        var param = GetParameter(game);
        var libraries = GetLibrary(game);
        
        libraries.Load();//Make sure libraries are loaded
        
        var javaArgs = param.JavaArgs;
        var gameArgs = param.GameArgs;
        var libArgs = 
            libraries.Libraries
            .Select(a => a.LibraryPath);
        
        var mainClass = param.MainClass;
        var assetsIndex = param.AssetsId;

        if (game is CustomizedGame { UseBaseGeneration: true } customized)
        {
            var baseParam = GetParameter(customized.InheritsFrom);
            var baseLib = GetLibrary(customized.InheritsFrom);
            assetsIndex = baseParam.AssetsId;
            javaArgs += baseParam.JavaArgs;
            gameArgs += baseParam.GameArgs;
            libArgs = libArgs.Concat(baseLib.Libraries
                .Select(a => a.LibraryPath));
        }

        var retArg = $"{javaArgs} {mainClass} {gameArgs}";
        return InlineArg(retArg, assetsIndex, libArgs, game);
    }

    private static string InlineArg(string arg, string assetsIndex, IEnumerable<string> libraries, GameBase game)
    {
        var libs = string.Join("${classpath_separator}", libraries) 
                      + $"{{classpath_separator}}{game.GameJarFilePath}";
        arg = arg.Replace("${natives_directory}", $"\"{game.NativeDirPath}\"");
        arg = arg.Replace("${assets_root}", $"\"{game.AssetsDirPath}\"");
        arg = arg.Replace("${assets_index_name}", assetsIndex);
        arg = arg.Replace("${classpath}", $"\"{libs}\"");
        arg = arg.Replace("${version_name}", game.Version);
        arg = arg.Replace("${version_type}", "release");
        arg = arg.Replace("${library_directory}", game.LibrariesDirPath);
        arg = Localize.CpLocalize(arg);
        Logger.Logger.Info($"Arguments Generated. Targeted Game:{game.GameFolderPath}");
        return "${jvm_argument} " + arg;
    }
}