using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.SystemInf;
using System.Diagnostics.Tracing;
using PixanKit.LaunchCore.Log;

namespace PixanKit.LaunchCore.GameModule
{
    /// <summary>
    /// Folder Class<br/> An Abstraction For A .minecraft Folder
    /// </summary>
    public class Folder:IToJSON
    {
        internal static List<Folder> Folders = [];

        /// <summary>
        /// The path of the folder. Like C:/Users/admin/AppData/Roaming/.minecraft
        /// </summary>
        public string FolderPath
        {
            get => _folderpath;
        }

        /// <summary>
        /// The path of the library folder. FolderPath + "/libraries"
        /// </summary>
        public string LibraryDirPath
        {
            get => FolderPath + "/libraries";
        }

        /// <summary>
        /// The path of the Assets folder. FolderPath + "/assets"
        /// </summary>
        public string AssetsDirPath
        {
            get => FolderPath + "/assets";
        }

        /// <summary>
        /// The path of the version folder. <c>FolderPath</c> + "/versions"
        /// </summary>
        public string VersionDirPath
        {
            get => FolderPath + "/versions";
        }

        /// <summary>
        /// An Alias Of The Folder<br/>
        /// Alias Will Help User Manage Their Games
        /// </summary>
        public string Alias { get; set; } = "";

        /// <summary>
        /// The Array Of The Minecraft Games In The <c>Folder</c>
        /// </summary>
        public GameBase[] Games
        {
            get => [.. _games];
        }

        /// <summary>
        /// The Count Of Minecraft Games In The <c>Folder</c>
        /// </summary>
        public int Count
        {
            get => _games.Count;
        }

        /// <summary>
        /// Provides the first Minecraft in the folder
        /// </summary>
        internal GameBase First
        {
            get => _games[0];
        }

        /// <summary>
        /// Provide the last Minecraft in the folder
        /// </summary>
        internal GameBase Last
        {
            get => _games[^1];
        }

        /// <summary>
        /// The <c>Launcher</c> That The <c>Folder</c> Belongs To
        /// </summary>
        public Launcher? Owner
        {
            get => _owner;
        }

        private List<GameBase> _games = [];

        private string _folderpath = "";

        private Launcher? _owner;

        /// <summary>
        /// Init The <c>Folder</c> With The Path.
        /// </summary>
        /// <param name="path">Folder Path, For Example:
        /// <c>"C:\\Users\\Admin\\AppData\\Roaming\\.minecrafy"</c></param>
        public Folder(string path)
        {
            _folderpath = path;
            AddSelf();
            InitGames();
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
            _folderpath = jData["path"]?.ToString() ?? throw new Exception("JSON parse Wrong");
            Alias = jData["alias"]?.ToString() ?? "${defaultalias}";
            AddSelf();
            InitGames();
        }

        /// <summary>
        /// Set The Owner <c>Launcher</c> Of The <c>Folder</c>
        /// </summary>
        /// <param name="launcher">The Owner <c>Launcher</c></param>
        public void SetOwner(Launcher launcher)
        {
            _owner = launcher;
        }

        /// <summary>
        /// Get the specific folder in the memory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Folder? FindFolder(string path)
        {
            foreach (var folder in Folders)
            {
                if (path.StartsWith(folder.FolderPath)) return folder;
            }
            return null;
        }

        private void AddSelf()
        {
            foreach (var folder in Folders) 
            {
                if (folder.FolderPath == _folderpath)
                    return;
            }
            Folders.Add(this);
        }

        private void InitGames()
        {
            _games.Clear();
            string[] dirs = Directory.GetDirectories(VersionDirPath);
            foreach (string dir in dirs) 
            {
                GameBase game;
                try {
                    game = Initors.GameInitor(dir) ?? throw new Exception();
                }
                catch(Exception ex)
                {
                    Logger.Error(ex.Message);
                    Logger.Error("\n" + ex.StackTrace);
                    Logger.Error($"{dir} Is Not A Minecraft Game");
                    continue;
                }
                 
                if (game != null)
                {
                    _games.Add(game);
                    game.SetOwner(this);
                    Logger.Info($"Folder {FolderPath} Add Game {game.GameJarFilePath}");
                    Launcher.GameLoad?.Invoke(game);
                }
            }
            Logger.Info($"Folder {FolderPath} Added");
        }

        /// <summary>
        /// This method adds the game
        /// </summary>
        /// <param name="game">The <c>GameBase</c> That You Need To Add</param>
        public void AddGame(GameBase game)
        {
            if (Owner != null) Owner.AddGame(game);
            else InternalAddGame(game);
        }

        /// <summary>
        /// This method removes the game.
        /// </summary>
        /// <param name="game">The <c>GameBase</c> That You Need To Remove</param>
        public void RemoveGame(GameBase game) 
        {
            if (Owner != null) Owner.RemoveGame(game);
            else InternalRemoveGame(game);
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
            foreach(GameBase game in _games)
            {
                if (game.Name == name) return game;
            }
            return null;
        }

        /// <summary>
        /// Find The Specific Version And Type Of A Game In The Folder
        /// </summary>
        /// <param name="version">The Version. Like <c>"1.14"</c></param>
        /// <param name="type">Type Of The Game. Like <c>GameType.Vanilla</c></param>
        /// <returns>Return The GameBase If Exists</returns>
        public GameBase? FindVersion(string version, GameType type) 
        {
            foreach (GameBase game in _games) 
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
            string[] dirs = Directory.GetDirectories(VersionDirPath);
            foreach (string dir in dirs)
            {
                foreach (GameBase game in _games) if (game.Name == dir) continue;
                var tmp = Initors.GameInitor(dir);
                if (tmp != null)
                {
                    _games.Add(tmp);
                    tmp.SetOwner(this);
                }
            }
        }

        /// <summary>
        /// This method will close every game.
        /// </summary>
        public void Close()
        {
            foreach (GameBase game in _games) { game.Close(); }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public JObject ToJSON()
        {
            return new JObject()
            {
                { "path" , _folderpath },
                { "alias" , Alias },

            };
        }

        internal void InternalAddGame(GameBase game)
        {
            if (Contains(game)) return;
            _games.Add(game);
            game.SetOwner(this);
            Launcher.GameAdd?.Invoke(game);
        }

        internal void InternalRemoveGame(GameBase game)
        {
            _games.Remove(game);
            Directory.Delete(game.GameFolderPath);
            Launcher.GameRemove?.Invoke(game);
        }
    }
}
