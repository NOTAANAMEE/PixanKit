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
        /// The path of the version folder. Path + "/versions"
        /// </summary>
        public string VersionDir
        {
            get => Path + "/versions";
        }

        /// <summary>
        /// An alias. Better for user
        /// </summary>
        public string Alias { get; set; } = "";

        /// <summary>
        /// The set of Minecraft games
        /// </summary>
        public GameBase[] Games
        {
            get => _games.ToArray();
        }

        public int Count
        {
            get => _games.Count;
        }

        /// <summary>
        /// Provides the first Minecraft in the folder
        /// </summary>
        public GameBase First
        {
            get => _games[0];
        }

        /// <summary>
        /// Provide the last Minecraft in the folder
        /// </summary>
        public GameBase Last
        {
            get => _games[^1];
        }

        /// <summary>
        /// The Launcher
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
        /// Init directly
        /// </summary>
        /// <param name="path"></param>
        public Folder(string path)
        {
            _path = path;
            
            AddSelf();
        }

        /// <summary>
        /// {
        /// "path":"C:/Users/Admin/AppData/Roaming/.minecraft"
        /// "alias":"New Folder"
        /// }
        /// </summary>
        /// <param name="jData"></param>
        public Folder(JObject jData) 
        {
            _path = jData["path"].ToString();
            Alias = jData["alias"].ToString();
            AddSelf();
            InitGames();
        }

        public void SetOwner(Launcher launcher)
        {
            _owner = launcher;
        }

        /// <summary>
        /// Get the library from the name
        /// </summary>
        /// <param name="name">org.lwjgl:lwjgl-freetype:3.3.3</param>
        /// <returns></returns>
        public LibraryBase? FindLibrary(string name)
        {
            return (_libraries.ContainsKey(name))? _libraries[name] : null;
        }

        /// <summary>
        /// Add the library
        /// </summary>
        /// <param name="library"></param>
        public void AddLibrary(LibraryBase library)
        {
            if (FindLibrary(library.Name) != null) return;
            _libraries.Add(library.Name, library);
        }

        /// <summary>
        /// Remove the library
        /// </summary>
        /// <param name="library"></param>
        public void RemoveLibrary(LibraryBase library) 
        {
            if (library.ReferenceCount > 0) throw new ArgumentException();
            if (FindLibrary(library.Name) == null) return;
            _libraries.Remove(library.Name);
            File.Delete(LibraryDir + $"/{library.Path}");
        }

        /// <summary>
        /// Remove the library reference.
        /// </summary>
        /// <param name="library"></param>
        public void RemoveLibraryReference(LibraryBase library)
        {
            if (!_libraries.ContainsValue(library)) throw new ArgumentException();
            library.ReferenceCount--;
            if (library.ReferenceCount == 0) RemoveLibrary(library);
        }

        /// <summary>
        /// If the library exists, add the reference. Else, add the library to _libraries and set the reference to 1
        /// </summary>
        /// <param name="library"></param>
        public void AddLibraryReference(LibraryBase library)
        {
            if (!_libraries.ContainsValue(library))
            {
                AddLibrary(library);
                library.ReferenceCount = 1;
            }
            else library.ReferenceCount ++;
        }

        /// <summary>
        /// Get the same name library. If exists, the reference will ++. Else, it will duplicate another instance.
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public LibraryBase? SameLibraryReference(LibraryBase library)
        {
            if (_libraries.ContainsKey(library.Name)) return _libraries[library.Name];
            LibraryBase tmp;
            switch (library.LibraryType)
            {
                case LibraryType.Ordinary:
                    tmp = (library as OrdinaryLibrary).Copy();
                    break;
                case LibraryType.Native:
                    tmp = (library as NativeLibrary).Copy();
                    break;
                case LibraryType.Mod:
                    tmp = (library as LoaderLibrary).Copy();
                    break;
                default: return null;
            }
            AddLibraryReference(tmp);
            return tmp;
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
                }
            }
        }

        /// <summary>
        /// This method adds the game
        /// </summary>
        /// <param name="game"></param>
        public void AddGame(GameBase game)
        {
            if (Owner != null) Owner.AddGame(game);
            else InternalAddGame(game);
        }

        /// <summary>
        /// This method removes the game.
        /// </summary>
        /// <param name="game"></param>
        public void RemoveGame(GameBase game) 
        {
            if (Owner != null) Owner.RemoveGame(game);
            else InternalRemoveGame(game);
        }

        /// <summary>
        /// This method will return whether the game exists in this folder
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool HasGame(GameBase game) 
            => _games.Contains(game);

        /// <summary>
        /// This method will return the game with specific name if exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameBase? FindGame(string name)
        {
            foreach(GameBase game in _games)
            {
                if (game.Name == name) return game;
            }
            return null;
        }

        public GameBase? FindVersion(string version, GameType type) 
        {
            foreach (GameBase game in _games) 
            {
                if (game.Version == version && game.GameType == type) return game;
            }
            return null;
        }

        /// <summary>
        /// This method will scan the folder again and add the game that does not exist in the _games 
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
        }

        internal void InternalRemoveGame(GameBase game)
        {
            game.folder = null;
            _games.Remove(game);
            foreach (var library in game.libraries)
            {
                RemoveLibraryReference(library);//Why did I do this?
            }
            Directory.Delete(game.GameFolder);
        }
    }
}
