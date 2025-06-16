using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core.Managers;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.Library;
using PixanKit.LaunchCore.PlayerModule.Player;
using PixanKit.LaunchCore.SystemInf;
using PixanKit.LaunchCore.Exceptions;

namespace PixanKit.LaunchCore.Extension;

/// <summary>
/// Provides custom initialization methods for games, players, and system settings.
/// </summary>
public static class Initers
{
    /// <summary>
    /// Gets or sets the instance of the custom game initializer.
    /// </summary>
    public static IGameIniter GameIniterInstance = new DefaultGameIniter();

    /// <summary>
    /// Gets or sets the instance of the custom setting modifier.
    /// </summary>
    public static ISettingModifier SettingModifier = new DefaultSettingModifier();

    /// <summary>
    /// Initializes a game instance using the specified path.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="name">The name of the game. </param>
    /// <returns>An instance of <see cref="GameBase"/> representing the initialized game.</returns>
    public static GameBase GameIniter(Folder folder, string name)
        => GameIniterInstance.InitGame(folder, name);

    /// <summary>
    /// A delegate for initializing a player from a JSON object.
    /// </summary>
    /// <remarks>
    /// This delegate allows customization of player initialization logic.
    /// </remarks>
    public static Func<JObject, PlayerBase?> PlayerIniter;

    /// <summary>
    /// A delegate for retrieving the system's memory allocation.
    /// </summary>
    /// <remarks>
    /// This delegate allows customization of memory allocation logic.
    /// </remarks>
    public static Func<long> GetMemory;

    /// <summary>
    /// Initializes static members of the <see cref="Initers"/> class.
    /// </summary>
    static Initers()
    {
        PlayerIniter += DefaultPlayerInitor;
        GetMemory += GetMem;
    }

    /// <summary>
    /// The default implementation for initializing a player from a JSON object.
    /// </summary>
    /// <param name="jData">A JSON object containing player data. Example:
    /// <code>
    /// {
    ///   "name": "PlayerName",
    ///   "uid": "UniqueID",
    ///   "type": "offline",
    ///   "refreshtoken": "RefreshToken",
    ///   "accesstoken": "AccessToken"
    /// }
    /// </code>
    /// </param>
    /// <returns>
    /// An instance of <see cref="PlayerBase"/> if the player type is recognized; otherwise, <c>null</c>.
    /// </returns>
    public static PlayerBase? DefaultPlayerInitor(JObject? jData)
    {
        if (jData == null) return null;
        return (jData["type"]?.ToString()) switch
        {
            "offline" => new OfflinePlayer(jData),
            "microsoft" => new MicrosoftPlayer(jData),
            _ => null,
        };
    }

    /// <summary>
    /// Retrieves the default memory allocation for the system.
    /// </summary>
    /// <returns>
    /// The allocated memory in megabytes. The value is determined based on the system's available memory,
    /// with a minimum of 2048 MB and a maximum of 10240 MB.
    /// </returns>
    public static long GetMem()
    {
        long minMemory = 2048; // 2GB
        long maxMemory = 10240; // 10GB
        var availableMemory = SysInfo.GetAvailableMemSize();
        var allocatedMemory = Math.Min(maxMemory, Math.Max(minMemory, availableMemory / 2));

        return allocatedMemory;
    }
}

/// <summary>
/// The default implementation of the <see cref="IGameIniter"/> interface.
/// </summary>
internal class DefaultGameIniter : IGameIniter
{
    /// <summary>
    /// Initializes a game instance from the specified path.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="name">The path to the game folder. For example: <c>"C:/Games/Minecraft"</c>.</param>
    /// <returns>
    /// An instance of <see cref="GameBase"/> representing the initialized game.
    /// </returns>
    public GameBase InitGame(Folder folder, string name)
    {
        var jsonPath = $"{folder.VersionDirPath}{name}/{name}.json";
        var jData = JObject.Parse(File.ReadAllText(jsonPath));
        var version = GameParameter.GetVersion(jData, out var modified);
        if (modified > 0)
            return InitCustomizedGame(folder, name, jData, version);

        var cache = GameManager.Instance?.GetFirstGameByVersion(version);
        if (cache != null)
        {
            return new VanillaGame(name, folder, cache.Params, cache.Libraries);
        }

        var param = GameParameter.CreateInstance(jData);
        var lib = new LibraryCollection(
            $"{folder.VersionDirPath}{name}/{name}.json");
        return new VanillaGame(name, folder, param, lib);
    }
    
    private GameBase InitCustomizedGame(Folder folder, string name, JObject jData, string version)
    {
        var thisParam = GameParameter.CreateInstance(jData);
        var thisLib = new LibraryCollection($"{folder.VersionDirPath}{name}/{name}.json");
        var cache = GameManager.Instance?.GetFirstGameByVersion(version);
        if (cache == null) throw new RedirectInitException();
        return JudgeOptifine(thisParam)?
            new CustomizedGame(name, folder, 
                cache.Params, thisParam,
                cache.Libraries, thisLib) :
            new ModdedGame(name, folder, 
                cache.Params, thisParam,
                cache.Libraries, thisLib);
    }

    /// <summary>
    /// Determines whether the game is an OptiFine-modified version.
    /// </summary>
    /// <param name="param"></param>
    /// <returns>
    /// <c>true</c> if the game is an OptiFine-modified version; otherwise, <c>false</c>.
    /// </returns>
    private bool JudgeOptifine(GameParameter param)
    {
        var entryClass = param.MainClass;
        if (entryClass != "net.minecraft.launchwrapper.Launch") return false;
        var forge = param.GameArgs.Contains("fml");
        var optifine = param.GameArgs.Contains("optifine");
        return !forge && optifine;
    }
}

internal class DefaultSettingModifier : ISettingModifier
{
    string ISettingModifier.Key => "";

    public void ReadValue(GameBase game, JToken? token)
    {
        // Do nothing as this is the default implementation.
    }

    public JToken WriteValue(GameBase game)
    {
        return "";
    }
}

