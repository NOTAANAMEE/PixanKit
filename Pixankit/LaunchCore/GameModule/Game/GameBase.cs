using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.GameModule.Folders;

namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
/// Represents the base class for managing Minecraft game instances.
/// </summary>
/// <remarks>
/// This abstract class provides core functionality for handling Minecraft game instances, 
/// including managing game files, libraries, settings, and runtime arguments. 
/// It serves as a foundation for derived classes that implement specific game types, 
/// such as modded or vanilla versions.
/// 
/// Key features of this class include:
/// - Managing paths for game resources (e.g., JAR files, libraries, and assets).
/// - Generating launch arguments for starting the game.
/// - Supporting runtime settings and configuration management.
/// - Handling native library extraction and version-specific operations.
/// 
/// Derived classes are expected to implement additional functionality as needed, 
/// while reusing the core capabilities provided by this base class.
/// </remarks>
public abstract partial class GameBase
{
    #region Properties
    public partial string Name => _name; 
        
    public partial string Description
    {
        get => Settingses.Description;
        set => Settingses.Description = value;
    }
        
    public partial string GameFolderPath => $"{_folder.VersionDirPath}{_name}/"; 
        
    public partial Folder Owner => _folder; 
        
    public partial string GameJarFilePath => $"{GameFolderPath}{Name}.jar"; 
        
    public partial string GameJsonFilePath => $"{GameFolderPath}{Name}.json"; 
        
    public partial string LibrariesDirPath => _folder.LibraryDirPath;
        
    public partial string AssetsDirPath => _folder.AssetsDirPath;
        
    public partial string GameRootFolderPath => _folder.FolderPath;
        
    public partial string Version => Params.Version;

    public partial string NativeDirPath => $"{GameFolderPath}{Name}-natives/";

    public partial string SettingsPath => GameFolderPath + Files.SettingsPath; 
        
    public partial GameType GameType { get => _gameType; protected set => _gameType = value; }

    
        
    public partial short MinimalJavaVersion => Params.JvmVersion;

    /// <summary>
    /// Gets the settings for this game instance.
    /// </summary>
    /// <remarks>
    /// The settings define Java version preferences, runtime folder behavior, and custom descriptions.
    /// </remarks>
    public GameSettings Settings => Settingses;
    #endregion

    #region Fields
    /// <summary>
    /// The parameters of the game instance.
    /// </summary>
    protected GameParameter Params;

    private readonly Folder _folder;

    private readonly string _name;

    private GameType _gameType = GameType.Vanilla;

    /// <summary>
    /// 
    /// </summary>
    protected string ReleaseType = "release";

    /// <summary>
    /// The reference of library
    /// </summary>
    protected LibrariesRef LibrariesRef;
    
    protected GameSettings Settingses;
    #endregion
        
    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="folder"></param>
    /// <param name="param"></param>
    /// <param name="libraries"></param>
    protected GameBase(string name, Folder folder, 
        GameParameter param, LibrariesRef libraries)
    {
        _name = name;
        _folder = folder;
        Params = param;
        LibrariesRef = libraries;
        SetSettings();
    }
    #endregion

    #region InitorUsingMethods
    private void SetSettings()
    {
        if (File.Exists(SettingsPath))
        {
            var settings = JObject.Parse(
                File.ReadAllText(SettingsPath));
            Settingses = new(settings);
        }
            
        Settingses = new();
    }
    #endregion

    #region Others
    public partial LibraryBase[] GetLibraries()
    {
        return LibrariesRef.Libraries;
    }
        
    protected virtual partial void LoadJson(JObject gameJdata)
    {

    }
    #endregion
        
    public virtual partial void Close()
    {
        var settingPath = SettingsPath;
        var directoryPath = Path.GetDirectoryName(settingPath) ?? "";

        Logger.Logger.Info($"{GameJarFilePath} Closing");

        if (directoryPath.Length != 0 && !Directory.Exists(directoryPath)) 
            Directory.CreateDirectory(directoryPath);
            
        using FileStream fs = new(settingPath, FileMode.Create);
        using StreamWriter sw = new(fs);
        sw.Write(Settingses.ToJObject().ToString());

        Logger.Logger.Info($"{GameJarFilePath} Closed. File Saved");
    }
}

/// <summary>
/// Different types of Minecraft
/// Mod:ModLoader like Fabric, Quilt, Liteloader, Forge and NeoForge
/// Optifine:Only With Optifine
/// Vanilla:No modloader or Optifine.
/// </summary>
public enum GameType
{
    /// <summary>
    /// Vanilla Game
    /// </summary>
    Vanilla,
    /// <summary>
    /// Customized Game. Usually optifine
    /// </summary>
    Customized,
    /// <summary>
    /// Mod Game
    /// </summary>
    Modded
}