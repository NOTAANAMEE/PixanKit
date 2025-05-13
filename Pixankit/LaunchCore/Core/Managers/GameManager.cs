using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Log;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;

namespace PixanKit.LaunchCore.Core
{
    /// <summary>
    /// Manages game folders and game instances for the launcher.
    /// </summary>
    public class GameManager
    {
        #region Properties
        /// <summary>
        /// Gets the collection of folders managed by the launcher.
        /// </summary>
        public IReadOnlyList<Folder> Folders => _folders.AsReadOnly();

        /// <summary>
        /// Gets or sets the default game to be launched.
        /// </summary>
        public GameBase? TargetGame { get; set; }

        private List<Folder> _folders = [];
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
            foreach (JToken jData in Files.FolderJData["children"] ?? new JObject())
            {
                Folder tmp = new((JObject)jData);
                _folders.Add(tmp);
                tmp.InitGames();
            }
            string tmpstr = (Files.FolderJData["target"] ?? "").ToString();
            if (tmpstr != "") TargetGame = FindGame(tmpstr);
            UpdateTargetGame();
            Logger.Info("Game Module Inited Successfully");
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
            return Contains(game.Owner) && (game.Owner?.Contains(game) ?? false);
        }

        /// <summary>
        /// Checks whether the specified folder exists in the folder collection.
        /// </summary>
        /// <param name="folder">The folder to check.</param>
        /// <returns><c>true</c> if the folder exists; otherwise, <c>false</c>.</returns>
        public bool Contains(Folder? folder)
        {
            if (folder == null) return false;
            return _folders.Contains(folder);
        }

        /// <summary>
        /// Adds a folder to the launcher.
        /// </summary>
        /// <param name="folder">The folder to add.</param>
        /// <exception cref="InvalidOperationException">Thrown if the folder has already been added.</exception>
        public void AddFolder(Folder folder)
        {
            foreach (Folder f in _folders)
            {
                if (f.FolderPath == folder.FolderPath) throw new InvalidOperationException("Folder has added before");
            }
            _folders.Add(folder);
            UpdateTargetGame();
            Logger.Info($"Folder {folder.FolderPath} Added");
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
            Folder? f = FindFolder(path);
            if (f != null) RemoveFolder(f);
        }

        /// <summary>
        /// Finds the folder with the specified path.
        /// </summary>
        /// <param name="path">The path of the folder to find.</param>
        /// <returns>The folder if found; otherwise, <c>null</c>.</returns>
        public Folder? FindFolder(string path)
        {
            foreach (Folder folder in _folders)
            {
                if (folder.FolderPath == path) return folder;
            }
            return null;
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
        /// Removes a game from its corresponding folder.
        /// </summary>
        /// <param name="game">The game to remove.</param>
        public void RemoveGame(GameBase game)
        {
            if (game.Owner == null) return;
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
            string folderpath = path[..path.LastIndexOf("/versions/")];
            string name = Path.GetDirectoryName(path)
                ?? throw new ArgumentException(path);
            Folder? res = FindFolder(folderpath);

            if (res == null) return null;
            return res.FindGame(name);
        }

        /// <summary>
        /// Updates the target game to ensure it is valid.
        /// </summary>
        private void UpdateTargetGame()
        {
            if (TargetGame is null) FirstGame();
            else if (TargetGame.Owner is null) throw new Exception();
            else if (!_folders.Contains(TargetGame.Owner)) FirstGame();
            else if (TargetGame.Owner.Contains(TargetGame)) return;
            else if (TargetGame.Owner.Count > 0) TargetGame = TargetGame.Owner.First;
            else FirstGame();
        }

        /// <summary>
        /// Sets the target game to the first available game in the folder collection.
        /// </summary>
        private void FirstGame()
        {
            foreach (Folder folder in _folders) if (folder.Count > 0)
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
            Logger.Info("Game Manager Closing");
            JArray folders = [];
            foreach (var folder in _folders)
            {
                folders.Add(folder.ToJSON());
                folder.Close();
            }
            return new JObject()
            {
                { "children", folders},
                { "target", TargetGame?.GameJarFilePath?? "" }
            };
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
        #endregion
    }
}
