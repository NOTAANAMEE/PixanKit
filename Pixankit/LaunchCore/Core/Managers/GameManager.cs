using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.LibraryData;
using System.Diagnostics.CodeAnalysis;

namespace PixanKit.LaunchCore.Core.Managers;

/// <summary>
/// Manages game folders and game instances for the launcher.
/// </summary>
public class GameManager
{
    private record GameParams(GameParameter Parameter, LibrariesRef Libraries);
        
    #region Properties
    /// <summary>
    /// Gets the collection of folders managed by the launcher.
    /// </summary>
    public IReadOnlyList<Folder> Folders => _folders.AsReadOnly();

    /// <summary>
    /// Gets or sets the default game to be launched.
    /// </summary>
    public GameBase? TargetGame { get; set; }

    private readonly List<Folder> _folders = [];
        
    private readonly Dictionary<string, GameParams> _gameRefs = [];
    #endregion

    #region Init
    /// <summary>
    /// Initializes a new instance of the <see cref="GameManager"/> class.
    /// </summary>
    internal GameManager()
    {
            
    }

    /// <summary>
    /// Initializes the game module by loading folder data from a JSON file.
    /// </summary>
    public void InitGameModule()
    {
        foreach (var jData in Files.FolderJData["children"] ?? new JObject())
        {
            Folder tmp = new((JObject)jData);
            _folders.Add(tmp);
            tmp.InitGames();
        }
        var tmpstr = (Files.FolderJData["target"] ?? "").ToString();
        if (tmpstr != "") TargetGame = FindGame(tmpstr);
        UpdateTargetGame();
        Logger.Logger.Info("Game Module Initialized Successfully");
    }
    #endregion

    #region Methods
    /// <summary>
    /// Checks whether the specified game exists in any folder.
    /// </summary>
    /// <param name="game">The game to check.</param>
    /// <returns><c>true</c> if the game exists; otherwise, <c>false</c>.</returns>
    public bool Contains(GameBase? game)
    {
        if (game == null) return false;
        return Contains(game.Owner) && game.Owner.Contains(game);
    }

    /// <summary>
    /// Checks whether the specified folder exists in the folder collection.
    /// </summary>
    /// <param name="folder">The folder to check.</param>
    /// <returns><c>true</c> if the folder exists; otherwise, <c>false</c>.</returns>
    public bool Contains(Folder? folder)
        =>folder != null && _folders.Contains(folder);
        

    /// <summary>
    /// Adds a folder to the launcher.
    /// </summary>
    /// <param name="folder">The folder to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if the folder has already been added.</exception>
    public void AddFolder(Folder folder)
    {
        foreach (var f in _folders)
        {
            if (f.FolderPath == folder.FolderPath) throw new InvalidOperationException("Folder has added before");
        }
        _folders.Add(folder);
        UpdateTargetGame();
        Logger.Logger.Info($"Folder {folder.FolderPath} Added");
    }

    /// <summary>
    /// Removes the specified folder from the launcher.
    /// </summary>
    /// <param name="folder">The folder to remove.</param>
    public void RemoveFolder(Folder folder)
    {
        if (!_folders.Contains(folder)) return;
        _folders.Remove(folder);
        UpdateTargetGame();
        OnFolderRemoved?.Invoke(folder);
    }

    /// <summary>
    /// Removes the folder with the specified path.
    /// </summary>
    /// <param name="path">The path of the folder to remove.</param>
    public void RemoveFolder(string path)
    {
        var f = FindFolder(path);
        if (f != null) RemoveFolder(f);
    }

    /// <summary>
    /// Finds the folder with the specified path.
    /// </summary>
    /// <param name="path">The path of the folder to find.</param>
    /// <returns>The folder if found; otherwise, <c>null</c>.</returns>
    public Folder? FindFolder(string path)
    {
        path = path.Replace("\\", "/");
        return _folders.
            FirstOrDefault(folder => path.StartsWith(folder.FolderPath));
    }

    /// <summary>
    /// Adds a game to its corresponding folder.
    /// </summary>
    /// <param name="game">The game to add.</param>
    /// <exception cref="NoFolderException">Thrown if the folder for the game cannot be found.</exception>
    public void AddGame(GameBase game)
    {
        game.Owner.AddGame(game);
        OnGameAdded?.Invoke(game.Owner, game);
        UpdateTargetGame();
    }

    /// <summary>
    /// Add a new game by its path
    /// </summary>
    /// <param name="path">the game folder path. For instance
    /// <c>C:\Users\Admin\.minecraft\versions\1.20.1</c>
    /// </param>
    /// <exception cref="Exception">
    /// Before adding Minecraft, add folder first!
    /// </exception>
    public void AddGame(string path)
    {
        var name = Path.GetFileName(path);
        var folder = FindFolder(path);
        if (folder == null) throw new Exception("Folder hasn't added before");
        AddGame(Initers.GameIniter(folder, name));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="Exception"></exception>
    public GameBase AddGameAndReturn(string path)
    {
        var name = Path.GetFileName(path);
        var folder = FindFolder(path);
        if (folder == null) throw new Exception("Folder hasn't added before");
        GameBase ret;
        AddGame( ret =Initers.GameIniter(folder, name));
        return ret;
    }

    /// <summary>
    /// Removes a game from its corresponding folder.
    /// </summary>
    /// <param name="game">The game to remove.</param>
    public void RemoveGame(GameBase game)
    {
        game.Owner.RemoveGame(game);
        OnGameRemoved?.Invoke(game.Owner, game);
        if (TargetGame == game) TargetGame = null;
        UpdateTargetGame();
    }

    /// <summary>
    /// Finds a game by its path.
    /// </summary>
    /// <param name="path">The path of the game directory.</param>
    /// <returns>The game if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown if the path is invalid.</exception>
    public GameBase? FindGame(string path)
    {
        path = path.Replace("\\", "/");
        var s = path[..path.LastIndexOf("/versions/", StringComparison.Ordinal)];
        var name = Path.GetFileName(path)
                   ?? throw new ArgumentException(path);
        var res = FindFolder(s);

        if (res == null) return null;
        return res.FindGame(name);
    }

    /// <summary>
    /// Updates the target game to ensure it is valid.
    /// </summary>
    private void UpdateTargetGame()
    {
        if (TargetGame is null || !_folders.Contains(TargetGame.Owner)) FirstGame();
        else if (TargetGame.Owner.Contains(TargetGame)) return;
        else if (TargetGame.Owner.Count > 0) TargetGame = TargetGame.Owner.First;
        else FirstGame();
    }

    /// <summary>
    /// Sets the target game to the first available game in the folder collection.
    /// </summary>
    private void FirstGame()
    {
        foreach (var folder in _folders.Where(folder => folder.Count > 0))
        {
            TargetGame = folder.First;
            return;
        }
    }

    /// <summary>
    /// Saves the folder data to a JSON object.
    /// </summary>
    /// <returns>A <see cref="JObject"/> containing the folder data.</returns>
    internal JObject Save()
    {
        Logger.Logger.Info("Game Manager Closing");
        JArray folders = [];
        foreach (var folder in _folders)
        {
            folders.Add(folder.ToJson());
            folder.Close();
        }
        return new JObject()
        {
            { "children", folders},
            { "target", TargetGame?.GameJarFilePath?? "" }
        };
    }
    #endregion

    #region GetParams

    /// <summary>
    /// 
    /// </summary>
    /// <param name="param"></param>
    /// <param name="libraries"></param>
    /// <exception cref="Exception"></exception>
    public void AddParam(GameParameter param, LibrariesRef libraries)
    {
        if (param.IsModified || _gameRefs.ContainsKey(param.Version)) 
            throw new Exception();
        _gameRefs.Add(param.Version, new GameParams(param, libraries));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    /// <param name="param"></param>
    /// <param name="libraries"></param>
    /// <returns></returns>
    public bool TryGetParam(string version, 
        [NotNullWhen(true)]out GameParameter? param, 
        [NotNullWhen(true)]out LibrariesRef? libraries)
    {
        if (_gameRefs.TryGetValue(version, out var p))
        {
            param = p.Parameter;
            libraries = p.Libraries;
            return true;
        }
        param = null;
        libraries = null;
        return false;
    }

    #endregion

    #region Events
    /// <summary>
    /// Occurs when a game is loaded from a file.
    /// </summary>
    public static Action<GameBase>? OnGameLoaded;

    /// <summary>
    /// Occurs when a new game is added to a folder.
    /// </summary>
    public static Action<Folder, GameBase>? OnGameAdded;

    /// <summary>
    /// Occurs when a game is removed from a folder.
    /// </summary>
    public static Action<Folder, GameBase>? OnGameRemoved;

    /// <summary>
    /// Occurs when the default game is changed.
    /// </summary>
    public static Action<GameBase?>? OnTargetGameChanged;

    /// <summary>
    /// Occurs when a new folder is added.
    /// </summary>
    public static Action<Folder>? OnFolderAdded;

    /// <summary>
    /// Occurs when a folder is removed.
    /// </summary>
    public static Action<Folder>? OnFolderRemoved;

    /// <summary>
    /// Occurs when the settings are read from a JSON object.
    /// </summary>
    public static Action<GameBase, JObject>? OnSettingsRead;
    #endregion
}