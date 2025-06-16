using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Exceptions;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.Folders;

/// <summary>
/// Folder Class<br/> An Abstraction For A .minecraft Folder
/// </summary>
public partial class Folder : IToJson
{
    #region Properties
    /// <summary>
    /// The path of the folder. Like C:/Users/admin/AppData/Roaming/.minecraft
    /// </summary>
    public string FolderPath => _folderPath;

    /// <summary>
    /// The path of the library folder. FolderPath + "/libraries"
    /// </summary>
    public string LibraryDirPath => FolderPath + "/libraries/";

    /// <summary>
    /// The path of the Assets folder. FolderPath + "/assets"
    /// </summary>
    public string AssetsDirPath => FolderPath + "/assets/";

    /// <summary>
    /// The path of the version folder. <c>FolderPath</c> + "/versions"
    /// </summary>
    public string VersionDirPath => FolderPath + "/versions/";

    /// <summary>
    /// An Alias Of The Folder<br/>
    /// Alias Will Help User Manage Their Games
    /// </summary>
    public string Alias { get; set; } = "";

    /// <summary>
    /// The Array Of The Minecraft Games In The <c>Folder</c>
    /// </summary>
    public GameBase[] Games => [.. _games];

    /// <summary>
    /// The Count Of Minecraft Games In The <c>Folder</c>
    /// </summary>
    public int Count => _games.Count;
    #endregion

    #region Fields
    /// <summary>
    /// Provides the first Minecraft in the folder
    /// </summary>
    internal GameBase First => _games[0];

    /// <summary>
    /// Provide the last Minecraft in the folder
    /// </summary>
    internal GameBase Last => _games[^1];

    /// <summary>
    /// The <c>Launcher</c> That The <c>Folder</c> Belongs To
    /// </summary>
    public static Launcher Owner => Launcher.Instance;

    private readonly List<GameBase> _games = [];

    private string _folderPath = "";
    #endregion

    #region Methods
    /// <summary>
    /// Init The <c>Folder</c> With The Path.
    /// </summary>
    /// <param name="path">Folder Path, For Example:
    /// <c>"C:\\Users\\Admin\\AppData\\Roaming\\.minecraft"</c></param>
    public Folder(string path)
    {
        _folderPath = path.Replace("\\", "/");
    }

    /// <summary>
    /// Init A <c>Folder</c> With JSON Object
    /// </summary>
    /// <param name="jData">
    /// JSON Object Of The Folder, For Example:
    /// <br/>
    /// <c>{<br/>
    /// "path":"C:/Users/Admin/AppData/Roaming/.minecraft",<br/>
    /// "alias":"New Folder"<br/>
    /// }</c></param>
    public Folder(JObject jData)
    {
        LoadFromJson(jData);
    }

    internal List<string> InitGames()
    {
        _games.Clear();
        var dirs = Directory.GetDirectories(VersionDirPath);
        List<string> reInit = [];
        foreach (var dir in dirs)
        {
            GameBase game;
            try
            {

                game =
                    Initers.GameIniter(this, Path.GetFileName(dir)) ??
                    throw new Exception();
            }
            catch (RedirectInitException)
            {
                reInit.Add(dir);
                continue;
            }
            catch (Exception ex)
            {
                Logger.Logger.Error(ex.Message);
                Logger.Logger.Error("\n" + ex.StackTrace);
                Logger.Logger.Error($"{dir} Is Not A Minecraft Game");
                continue;
            }
            _games.Add(game);
            Logger.Logger.Info("Game Initialized");
        }
        Logger.Logger.Info($"Folder {FolderPath} Added");
        return reInit;
    }
    
    internal void AddGame(GameBase game)
    {
        _games.Add(game);
        OnGameChanged?.Invoke();
    }

    internal void RemoveGame(GameBase game)
    {
        _games.Remove(game);
        OnGameChanged?.Invoke();
    }

    /// <summary>
    /// This method will return whether the game exists in this folder
    /// </summary>
    /// <param name="game">The <c>GameBase</c> You Want To Check</param>
    /// <returns></returns>
    public bool Contains(GameBase game)
        => _games.Contains(game);

    /// <summary>
    /// This method will return the game with specific name if exists.
    /// </summary>
    /// <param name="name">The Name Of The Game</param>
    /// <returns>If Exists, Return The Game. Else, Return <c>null</c></returns>
    public GameBase? FindGame(string name)
    {
        foreach (var game in _games)
        {
            if (game.Name == name) return game;
        }
        return null;
    }

    /// <summary>
    /// Find The Specific Version And Type Of Game In The Folder
    /// </summary>
    /// <param name="version">The Version. Like <c>"1.14"</c></param>
    /// <param name="type">Type Of The Game. Like <c>GameType.Vanilla</c></param>
    /// <returns>Return The GameBase If Exists</returns>
    public GameBase? FindVersion(string version, GameType type)
    {
        foreach (var game in _games)
        {
            if (game.Version == version && game.GameType == type) return game;
        }
        return null;
    }

    /// <summary>
    /// This method will scan the folder again and add the game that does not exist in the <c>_games</c> 
    /// </summary>
    public void Scan()
    {
        var dirs = Directory.GetDirectories(VersionDirPath);
        foreach (var dir in dirs)
        {
            var tmpdir = dir.Replace("\\", "/");
            if (_games.Any(game => game.GameFolderPath == tmpdir)) continue;
            AddGame(
                Initers.GameIniter(this, Path.GetFileName(dir)));
        }
    }

    /// <summary>
    /// This method will close every game.
    /// </summary>
    public void Close()
    {
        foreach (var game in _games) { game.Close(); }
    }
    #endregion

    #region Events
    /// <summary>
    /// Event Triggered When A Game Is Added/Removed
    /// </summary>
    public Action? OnGameChanged;
    #endregion
}