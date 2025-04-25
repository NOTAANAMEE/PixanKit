using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Log;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;

namespace PixanKit.LaunchCore.Core
{
    public class GameManager
    {
        #region Singleton
        private static Lazy<GameManager> _instance = new(() => new GameManager());

        public static GameManager Instance => _instance.Value;
        #endregion

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
        private GameManager()
        {
            InitGameModule();
            Logger.Info("Game Module Inited Successfully");
        }

        private void InitGameModule()
        {
            List<Folder> folders = [];
            foreach (JToken jData in Files.FolderJData["children"] ?? new JObject())
            {
                Folder tmp = new((JObject)jData);
                folders.Add(tmp);
            }
            _folders = folders;
            string tmpstr = (Files.FolderJData["target"] ?? "").ToString();
            if (tmpstr != "") TargetGame = FindGame(tmpstr);
            UpdateTargetGame();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks whether the game is one of the games that exists in any foldr
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool Contains(GameBase? game)
        {
            if (game == null) return false;
            return Contains(game.Owner) && (game.Owner?.Contains(game) ?? false);
        }

        /// <summary>
        /// Checks whether the folder is in the folder collection
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool Contains(Folder? folder)
        {
            if (folder == null) return false;
            return _folders.Contains(folder);
        }

        /// <summary>
        /// Add a folder to the Launcher
        /// </summary>
        /// <param name="folder"></param>
        /// <exception cref="InvalidOperationException"> Do not add a folder which is added</exception>
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
        /// Remove the folder.
        /// </summary>
        /// <param name="folder"></param>
        public void RemoveFolder(Folder folder)
        {
            if (!_folders.Contains(folder)) return;
            _folders.Remove(folder);
            UpdateTargetGame();
            OnFolderRemoved?.Invoke(folder);
        }

        /// <summary>
        /// Remove the foder with specific path
        /// </summary>
        /// <param name="path"></param>
        public void RemoveFolder(string path)
        {
            Folder? f = FindFolder(path);
            if (f != null) RemoveFolder(f);
        }

        /// <summary>
        /// Returns the folder with specific path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Folder? FindFolder(string path)
        {
            foreach (Folder folder in _folders)
            {
                if (folder.FolderPath == path) return folder;
            }
            return null;
        }

        /// <summary>
        /// Add the game to the folder. This method will automatically judge the folder.
        /// </summary>
        /// <param name="game"></param>
        /// <exception cref="NoFolderException"></exception>
        public void AddGame(GameBase game)
        {
            game.Owner.AddGame(game);
            OnGameAdded?.Invoke(game.Owner, game);
            UpdateTargetGame();
        }

        /// <summary>
        /// Remove the game from the folder. This method will automatically judge the folder.
        /// </summary>
        /// <param name="game"></param>
        public void RemoveGame(GameBase game)
        {
            if (game.Owner == null) return;
            game.Owner.RemoveGame(game);
            OnGameRemoved?.Invoke(game.Owner, game);
            if (TargetGame == game) TargetGame = null;
            UpdateTargetGame();
        }

        /// <summary>
        /// Get The Game From Its Path
        /// </summary>
        /// <param name="path">The Dir Of The Game</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GameBase? FindGame(string path)
        {
            string folderpath = path[..path.LastIndexOf("/versions/")];
            string name = Path.GetDirectoryName(path)
                ?? throw new ArgumentException(path);
            Folder? res = FindFolder(folderpath);

            if (res == null) return null;
            return res.FindGame(name);
        }

        private void UpdateTargetGame()
        {
            if (TargetGame is null) FirstGame();
            else if (TargetGame.Owner is null) throw new Exception();
            else if (!_folders.Contains(TargetGame.Owner)) FirstGame();
            else if (TargetGame.Owner.Contains(TargetGame)) return;
            else if (TargetGame.Owner.Count > 0) TargetGame = TargetGame.Owner.First;
            else FirstGame();
        }

        private void FirstGame()
        {
            foreach (Folder folder in _folders) if (folder.Count > 0)
                {
                    TargetGame = folder.First;
                    return;
                }
        }

        internal JObject SaveFolderData()
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

        public static Action<Folder, GameBase>? OnGameAdded;

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
