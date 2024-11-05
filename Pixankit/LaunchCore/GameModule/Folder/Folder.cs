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

namespace PixanKit.LaunchCore.GameModule
{
    internal delegate void FinishInited();

    /// <summary>
    /// Folder Class<br/> An Abstraction For A .minecraft Folder
    /// </summary>
    public class Folder:IToJSON
    {
        internal static List<Folder> Folders = new();

        /// <summary>
        /// The path of the folder. Like C:/Users/admin/AppData/Roaming/.minecraft
        /// </summary>
        public string Path
        {
            get => _path;
        }

        /// <summary>
        /// The path of the library folder. Path + "/libraries"
        /// </summary>
        public string LibraryDir
        {
            get => Path + "/libraries";
        }

        /// <summary>
        /// The path of the Assets folder. Path + "/assets"
        /// </summary>
        public string AssetsDir
        {
            get => Path + "/assets";
        }

        /// <summary>
        /// The path of the version folder. <c>Path</c> + "/versions"
        /// </summary>
        public string VersionDir
        {
            get => Path + "/versions";
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
            get => _games.ToArray();
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

        internal FinishInited GamesInited = new(() => { });

        private List<GameBase> _games = new();

        private Dictionary<string, LibraryBase> _libraries = new();

        private string _path = "";

        private Launcher? _owner;

        /// <summary>
        /// Init The <c>Folder</c> With The Path.
        /// </summary>
        /// <param name="path">Folder Path, For Example:
        /// <c>"C:\\Users\\Admin\\AppData\\Roaming\\.minecrafy"</c></param>
        public Folder(string path)
        {
            _path = path;
            AddSelf();
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
            _path = jData["path"].ToString();
            Alias = jData["alias"].ToString();
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
        /// Get The Library In The Folder With The Name
        /// </summary>
        /// <param name="name">The Name Of A Library From The Minecraft JSON File, For Example:
        /// <br/><c>"org.lwjgl:lwjgl-freetype:3.3.3"</c></param>
        /// <returns>If Exists, Return The Library. Else, Return <c>null</c></returns>
        public LibraryBase? FindLibrary(string name)
        {
            return (_libraries.ContainsKey(name))? _libraries[name] : null;
        }

        /// <summary>
        /// Remove A Library Reference.<br/>
        /// It Will Make The <c>LibraryBase.ReferenceCount</c> Decrement By 1
        /// </summary>
        /// <param name="library">The <c>LibraryBase</c> Reference That You Want To Remove</param>
        public void RemoveLibrary(LibraryBase library)
        {
            if (!_libraries.ContainsValue(library)) throw new ArgumentException();
            library.ReferenceCount--;
            if (library.ReferenceCount == 0) _libraries.Remove(library.Name);
        }

        /// <summary>
        /// If the library exists, add the reference.<br/>
        /// Else, add the library to <c>_libraries</c> and set the reference to 1<br/>
        /// As The Program Cannot Decide Which Library the <c>LibraryBase</c> Is, You Need To Init It 
        /// On Your Own
        /// </summary>
        /// <param name="library"><c>LibraryBase</c> You Want To Add Reference</param>
        public LibraryBase AddLibrary(LibraryBase library)
        {
            if (_libraries.ContainsKey(library.Name))
            {
                _libraries[library.Name].ReferenceCount++;
                return library;
            }
            else
            {
                if (!library.Path.StartsWith(this.LibraryDir)) 
                { 
                    library = library.Copy();
                    library.SetFolder(this);
                }
                _libraries.Add(library.Name, library);
                library.ReferenceCount = 1;
                return library;
            }
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
                if (path.StartsWith(folder.Path)) return folder;
            }
            return null;
        }

        private void AddSelf()
        {
            foreach (var folder in Folders) 
            {
                if (folder.Path == _path)
                    return;
            }
            Folders.Add(this);
        }

        private void InitGames()
        {
            _games.Clear();
            string[] dirs = Directory.GetDirectories(Localize.PathLocalize(VersionDir));
            foreach (string dir in dirs) 
            {
                var tmp = Initors.GameInitor(dir);
                if (tmp != null)
                {
                    _games.Add(tmp);
                    tmp.SetOwner(this);
                    Launcher.GameLoad?.Invoke(tmp);
                }
            }
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
        public bool HasGame(GameBase game) 
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
        /// <param name="type">Type Of The Game. Like <c>GameType.Ordinary</c></param>
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
            string[] dirs = Directory.GetDirectories(Localize.PathLocalize(VersionDir));
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
                { "path" , _path },
                { "alias" , Alias },

            };
        }

        internal void InternalAddGame(GameBase game)
        {
            if (HasGame(game)) return;
            _games.Add(game);
            game.SetOwner(this);
            Launcher.GameAdd?.Invoke(game);
        }

        internal void InternalRemoveGame(GameBase game)
        {
            game.folder = null;
            _games.Remove(game);
            foreach (var library in game.libraries)
            {
                RemoveLibrary(library);//Why did I do this?
            }
            Directory.Delete(game.GameFolder);
            Launcher.GameRemove?.Invoke(game);
        }
    }
}
