using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Game
{
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
    public abstract class GameBase
    {
        #region Properties
        /// <summary>
        /// Gets the name of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// This is derived from the folder name containing the game files.
        /// </remarks>
        public string Name { get => System.IO.Path.GetFileName(_path) ?? ""; }

        /// <summary>
        /// Gets or sets the description of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// Default value is "Minecraft" if not specified.
        /// </remarks>
        public string Description 
        { 
            get => (Settings["description"]?? "Minecraft").ToString();
            set => Settings["description"] = value;
        }

        /// <summary>
        /// Gets the folder where the main game files (e.g., game.jar) are located.
        /// </summary>
        public string GameFolder { get =>_path; }

        /// <summary>
        /// Gets the owner folder of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// This represents the parent directory of the game folder.
        /// </remarks>
        public Folder? Owner { get => folder; }

        /// <summary>
        /// Gets the full path to the game JAR file.
        /// </summary>
        /// <remarks>
        /// Combines the <see cref="GameFolder"/> and the <see cref="Name"/> to locate the JAR file.
        /// </remarks>
        public string Path { get => _path + $"/{Name}.jar"; }

        /// <summary>
        /// Gets the full path to the game's JSON configuration file.
        /// </summary>
        /// <remarks>
        /// This file stores metadata about the game instance.
        /// </remarks>
        public string JsonPath { get => _path + $"/{Name}.json"; }

        /// <summary>
        /// Gets or sets the folder used to store world data, mods, and other runtime files.
        /// </summary>
        /// <remarks>
        /// Also known as the "game directory".
        /// </remarks>
        public string RunningDir
        {
            get => GetRunningFolder();
            set => Settings["runningfolder"] = value;
        }

        /// <summary>
        /// Gets the folder where library files are stored.
        /// </summary>
        /// <remarks>
        /// Defaults to the global libraries folder if no specific owner folder is defined.
        /// </remarks>
        public string LibraryDir
        {
            get => (folder == null)? RootDir + "/libraries":folder.LibraryDir;
        }

        /// <summary>
        /// Gets the folder where texture packs and assets are stored.
        /// </summary>
        /// <remarks>
        /// Defaults to the global assets folder derived from the Minecraft directory structure.
        /// </remarks>
        public string AssetsDir 
        { 
            get => (folder == null)? _path.Remove(_path.LastIndexOf("/versions/")) + "/assets"
                :folder.AssetsDir; 
        }

        /// <summary>
        /// Gets the root folder of the Minecraft installation.
        /// </summary>
        /// <remarks>
        /// This is the base directory containing all versions and global assets.
        /// </remarks>
        public string RootDir{ get => _path.Remove(_path.LastIndexOf("/versions/")); }

        /// <summary>
        /// Gets the Minecraft version for this game instance.
        /// </summary>
        public string Version { get => _version; }

        /// <summary>
        /// Gets the folder where native binary libraries are stored.
        /// </summary>
        /// <remarks>
        /// These binaries are required for platform-specific functionality.
        /// </remarks>
        public string NativeDir { get => _path + $"/{Name}-natives"; }

        /// <summary>
        /// Gets the path to the settings configuration file.
        /// </summary>
        /// <remarks>
        /// The settings file contains user and game-specific configurations.
        /// </remarks>
        public string SettingsPath { get => _path + Files.SettingsPath; }

        /// <summary>
        /// Gets the folder where crash reports are stored.
        /// </summary>
        /// <remarks>
        /// Crash reports are saved as compressed tar.gz files in this directory.
        /// </remarks>
        public string CrashReportDir { get => RunningDir + "/crash-reports"; }

        /// <summary>
        /// Gets the type of the game instance.
        /// </summary>
        /// <remarks>
        /// The game type defines whether the instance is a vanilla, modded, or other variant.
        /// </remarks>
        public GameType GameType { get => _gameType; }

        /// <summary>
        /// Gets the settings for this game instance.
        /// </summary>
        /// <remarks>
        /// The settings define Java version preferences, runtime folder behavior, and custom descriptions.
        /// </remarks>
        public JObject Settings = new()
        {
            { "java", "overall"},//"overall": the same as the overall settings, "specified": Should be the same version, "closest": The closest version(Bigger / equal), "newest": The largest version, default: user specified
            { "argument", "overall" },//"overall": the same as the overall settings, default:user specified
            { "runningfolder", "self" }, //"overall":the same as the overall settings, "self": self folder defult: user specified
            { "description", "A Minecraft Game" }
        };
        #endregion

        #region Fields
        private JObject tmpdata = new();

        internal Folder? folder = null;

        internal string _path = "";

        internal string _version = "";

        internal GameType _gameType;

        internal List<LibraryBase> libraries = new();

        internal string className = "";

        internal string gameArguments = "";

        internal string javaArguments = "";

        internal short javaVersion = 8;

        internal string assetsID = "";

        internal string releaseType = "";
        #endregion

        #region Initors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameBase"/> class.
        /// </summary>
        /// <param name="path">The path to the game folder.</param>
        /// <remarks>
        /// This constructor initializes the game instance from the specified path, 
        /// reading additional configuration if needed.
        /// </remarks>
        protected GameBase(string path):this(path, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBase"/> class with a JSON configuration file.
        /// </summary>
        /// <param name="path">
        /// The path to the game folder. For example: 
        /// <c>"C:/Users/Admin/AppData/Roaming/.minecraft/versions/1.20.1"</c>.
        /// </param>
        /// <param name="jData">A JSON object containing game-specific configuration details.</param>
        /// <remarks>
        /// Use this constructor when you already have a JSON object to initialize the game instance.
        /// </remarks>
        public GameBase(string path, JObject jData):this(path, false)
        {
            tmpdata = jData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBase"/> class with optional file-based initialization.
        /// </summary>
        /// <param name="path">
        /// The path to the game folder. For example:
        /// <c>"C:/Users/Admin/AppData/Roaming/.minecraft/versions/1.14"</c>.
        /// </param>
        /// <param name="initFromFile">
        /// If <c>true</c>, the constructor reads configuration data from a file in the specified path. 
        /// Otherwise, no file is read.
        /// </param>
        /// <remarks>
        /// This constructor is typically used to create a game instance with an optional initialization step based on existing files.
        /// </remarks>
        protected GameBase(string path, bool initFromFile) 
        {
            _path = path;
            if (initFromFile)
            {
                tmpdata = ReadJObj(path);

            }
            Logger.Info($"Game Base Added. Path:{_path}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBase"/> class with only JSON data.
        /// </summary>
        /// <param name="jData">A JSON object containing game-specific configuration details.</param>
        /// <remarks>
        /// This constructor is useful for creating a game instance based solely on JSON data, 
        /// without specifying a path.
        /// </remarks>
        protected GameBase(JObject jData):this("", jData) { }
        #endregion

        #region InitorUsingMethods
        /// <summary>
        /// Init JSON Data From Outside
        /// </summary>
        /// <param name="jData"></param>
        public void InitJData(JObject jData)
        {
            tmpdata = jData;
            InitJData();
            
        }

        /// <summary>
        /// Init JSON Data From tmpdata
        /// </summary>
        protected void InitJData()
        { 
            var jData = tmpdata;
            className = (jData["mainClass"] ?? "net.minecraft.client.main.Main.").ToString();
            SetLibrary(jData);
            SetGameArgument(jData);
            SetJVMArguments(jData);
            
            //Set The Version. Real Version
            if (jData["inheritsfrom"] != null) _version = (jData["inheritsfrom"] ?? "").ToString();
            _version = (jData["id"] ?? "").ToString();
            releaseType = (jData["type"] ?? "release").ToString();
            LoadJSON(tmpdata);
            SetSettings();
            tmpdata = new();
        }

        /// <summary>
        /// Set the folder of the game
        /// </summary>
        /// <param name="owner">folder. Should be the actual folder that it is in</param>
        public virtual void SetOwner(Folder owner)
        {
            if (_path.StartsWith(owner.Path))
            folder = owner;
            InitJData();
        }

        private void SetSettings()
        {
            string path = Localize.PathLocalize(SettingsPath);
            if (File.Exists(path))
                Settings = JObject.Parse(
                    File.ReadAllText(path));
        }

        private void SetLibrary(JObject jData)
        {
            foreach (JToken token in (jData["libraries"] ?? new JObject()))
            {
                //System Support?
                if (!SystemSupport(token)) continue;

                switch (LibraryBase.GetLibraryType(token))
                {
                    case LibraryType.Original:
                        libraries.Add(FolderAddLibrary(new OriginalLibrary(token)));
                        break;
                    case LibraryType.Native:
                        libraries.Add(FolderAddLibrary(new NativeLibrary(token)));
                        if ((token["downloads"] as JObject ?? new JObject()).Count > 1) 
                            libraries.Add(FolderAddLibrary(new OriginalLibrary(token)));
                        break;
                    case LibraryType.Mod:
                        libraries.Add(FolderAddLibrary(new LoaderLibrary(token)));
                        break;
                }
            }
            Logger.Info($"Libraries Added. Number:{libraries.Count}");
        }

        private bool SystemSupport(JToken libraryToken)
        {
            return LibraryBase.GetAllowedSystem(libraryToken).Contains(SystemInformation.OSName);
        }

        private LibraryBase FolderAddLibrary(LibraryBase library)
        {
            if (folder == null) return library;
            else return folder.AddLibrary(library);
        }

        internal virtual void SetGameArgument(JObject jData)
        {
            if (jData["minecraftArguments"] != null) 
            {
                gameArguments = (jData["minecraftArguments"] ?? 1).ToString();
            }
            else
            {
                foreach(JToken token in (jData["arguments"] ?? new JObject())["game"]?? new JObject())
                {
                    if (token.Type != JTokenType.String) continue;
                    gameArguments += token.ToString() + " ";
                }
            }
        }

        internal virtual void SetJVMArguments(JObject jData)
        {
            string tmp = "[{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"osx\"}}],\"value\": [\"-XstartOnFirstThread\"]},{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"windows\"}}],\"value\": \"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump\"},{\"rules\": [{\"action\": \"allow\",\"os\": {\"arch\": \"x86\"}}],\"value\": \"-Xss1M\"},\"-Djava.library.path=${natives_directory}\"" +
                ",\"-Dminecraft.launcher.brand=${launcher_name}\",\"-Dminecraft.launcher.version=${launcher_version}\",\"-cp\",\"${classpath}\"]";
            JArray? jvmargArray;
            if (jData["arguments"] != null && (jData["arguments"] ?? new JObject())["jvm"] != null)
            {
                jvmargArray = (jData["arguments"] ?? new JObject())["jvm"] as JArray;
            }
            else
            {
                jvmargArray = JArray.Parse(tmp);
            }
            if (jvmargArray == null) return;
            foreach(JToken token in jvmargArray)
            {
                if (token.Type == JTokenType.String) 
                {
                    string tok = token.ToString();
                    if (tok.Contains(' ')) tok = "\"" + tok + "\"";
                    javaArguments += tok + " "; 
                    continue; 
                }
                if (LibraryBase.GetAllowedSystem(token).Contains(SystemInformation.OSName))
                {
                    string tmpstr;
                    if ((token["value"]?? 0).Type == JTokenType.String) tmpstr = (token["value"] ?? 1).ToString();
                    else tmpstr = string.Join(' ', token["value"] as JArray?? new JArray());
                    javaArguments += tmpstr + " ";
                }
            }
        }

        private static JObject ReadJObj(string path)
        {
            JObject jData;
            {
                string tmpcontent = File.ReadAllText(path);
                jData = JObject.Parse(tmpcontent);
            }
            return jData;
        }
        #endregion

        #region LaunchMethods
        /// <summary>
        /// Generates the launch arguments for the game.
        /// </summary>
        /// <returns>A formatted string containing the full launch arguments.</returns>
        /// <remarks>
        /// This method replaces placeholders like <c>${natives_directory}</c> with actual paths
        /// and applies localization. It also incorporates JVM and game-specific arguments.
        /// </remarks>
        public virtual string GetLaunchArgument()
        {
            string arg = GetJVMArguments() + $" {className} " + GetGameArguments();
            arg = arg.Replace("${natives_directory}", $"\"{NativeDir}\"");
            arg = arg.Replace("${game_directory}", $"\"{RunningDir}\"");
            arg = arg.Replace("${assets_root}", $"\"{AssetsDir}\"");
            arg = arg.Replace("${assets_index_name}", GetAssetsID());
            arg = arg.Replace("${classpath}", "\"" + GetCPArgs() + "\"");
            arg = arg.Replace("${version_name}", Version);
            arg = arg.Replace("${version_type}", releaseType);
            arg = arg.Replace("${library_directory}", LibraryDir);
            arg = Localize.CPLocalize(arg);
            Logger.Info($"Arguments Generated. Targeted Game:{_path}");
            if (Settings == null) return arg;
            switch ((Settings["argument"] ?? 1).ToString())
            {
                case "overall":
                    arg = "${arguments} " + arg;
                    break;
                default:
                    arg = (Settings["arguments"] ?? 1).ToString() + " " + arg;
                    break;
            }
            return arg;
        }

        /// <summary>
        /// Checks whether the game is ready to launch.
        /// </summary>
        /// <returns><c>true</c> if the game can launch; otherwise, <c>false</c>.</returns>
        public virtual bool LaunchCheck() => true;

        /// <summary>
        /// Retrieves the Java Virtual Machine (JVM) arguments.
        /// </summary>
        /// <returns>A string containing the JVM arguments.</returns>
        protected virtual string GetJVMArguments()
            => javaArguments;

        /// <summary>
        /// Retrieves the game-specific arguments.
        /// </summary>
        /// <returns>A string containing the game arguments.</returns>
        protected virtual string GetGameArguments()
            => gameArguments;

        /// <summary>
        /// Retrieves game arguments for the same version game within the folder.
        /// </summary>
        /// <returns>A string containing the game arguments.</returns>
        /// <exception cref="Exception">Thrown if the same version game cannot be found.</exception>
        protected string SameVersionGameArguments()
        {
            var target = Owner.FindVersion(_version, GameType.Original);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetGameArguments();
        }

        /// <summary>
        /// Retrieves the assets ID for the game.
        /// </summary>
        /// <returns>A string containing the assets ID.</returns>
        protected virtual string GetAssetsID()
            => assetsID;

        /// <summary>
        /// Retrieves the assets ID for the same version game within the folder.
        /// </summary>
        /// <returns>A string containing the assets ID.</returns>
        /// <exception cref="Exception">Thrown if the same version game cannot be found.</exception>
        protected string SameVersionAssetsID()
        {
            var target = Owner.FindVersion(_version, GameType.Original);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetAssetsID();

        }

        /// <summary>
        /// Retrieves the classpath arguments for the game.
        /// </summary>
        /// <returns>
        /// A string containing the classpath arguments, excluding the game JAR file.
        /// </returns>
        /// <remarks>
        /// The classpath includes library paths and the main game JAR path.
        /// </remarks>
        protected virtual string GetCPArgs()
        {
            string classpath = "";
            foreach (LibraryBase library in libraries)
            {
                classpath += library.Path + Localize.LocalParser;
            }
            classpath += Path;
            return classpath;
        }

        /// <summary>
        /// Retrieves the classpath arguments for the same version game within the folder.
        /// </summary>
        /// <returns>A string containing the classpath arguments.</returns>
        /// <exception cref="Exception">Thrown if the same version game cannot be found.</exception>
        protected string SameVersionCPArgs()
        {
            var target = Owner.FindVersion(_version, GameType.Original);
            if (target == null) { 
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version"); 
            }
            return target.GetCPArgs();
        }

        /// <summary>
        /// Decompresses native libraries required for the game.
        /// </summary>
        /// <remarks>
        /// This method extracts all native libraries to the <c>NativeDir</c> folder.
        /// It skips non-native libraries.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task Decompress()
        {
            foreach(LibraryBase library in libraries)
            {
                if (library.LibraryType != LibraryType.Native) continue;
                await Task.Run(
                    () => { 
                        (library as NativeLibrary??new NativeLibrary()).Extract( NativeDir); 
                    }
                );
            }
        }

        private string GetRunningFolder()
        {
            if (Settings == null) return "";
            string runningfolder = (Settings["runningfolder"] ?? 1).ToString();
            string ret;
            switch (runningfolder)
            {
                case "overall":
                    if (Owner == null || Owner.Owner == null) throw new Exception("Can't use overall");
                    Settings["runningfolder"] = Owner.Owner.Settings["runningfolder"];
                    ret = GetRunningFolder();
                    Settings["runningfolder"] = JToken.FromObject("overall");
                    break;
                case "self":
                    ret = _path;
                    break;
                default:
                    ret = runningfolder;
                    break;
            }
            return ret;
        }
        #endregion

        #region Others
        /// <summary>
        /// Retrieves all libraries associated with this game instance.
        /// </summary>
        /// <returns>An array of <see cref="LibraryBase"/> objects representing the libraries.</returns>
        /// <remarks>
        /// This method returns the complete list of libraries required for the game, including runtime and mod libraries.
        /// </remarks>
        public virtual LibraryBase[] GetLibraries()
        {
            return libraries.ToArray();
        }

        /// <summary>
        /// Retrieves libraries for the same version game within the folder.
        /// </summary>
        /// <returns>A list of <see cref="LibraryBase"/> objects representing the libraries.</returns>
        /// <exception cref="Exception">
        /// Thrown when the same version game cannot be found in the folder.
        /// </exception>
        /// <remarks>
        /// Use this method when you need to retrieve the libraries from a game version
        /// that matches the current version but exists in the same folder.
        /// </remarks>
        protected List<LibraryBase> SameVersionLibraries()
        {
            var target = Owner.FindVersion(_version, GameType.Original);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.libraries;
        }

        /// <summary>
        /// Initializes the game instance with the provided JSON configuration.
        /// </summary>
        /// <param name="gameJdata">A JSON object containing the game configuration.</param>
        /// <remarks>
        /// This method allows convenient subclass initialization by loading configuration details 
        /// such as assets, libraries, and runtime settings from a JSON file.
        /// </remarks>
        protected virtual void LoadJSON(JObject gameJdata)
        {

        }
        #endregion

        /// <summary>
        /// Saves the current settings to a file and closes the game instance.
        /// </summary>
        /// <remarks>
        /// This method writes the <see cref="Settings"/> object to the settings file specified by <see cref="SettingsPath"/>.
        /// If the settings directory does not exist, it will be created. 
        /// Proper logging is performed before and after saving the file.
        /// </remarks>
        /// <exception cref="IOException">
        /// Thrown if an error occurs while writing to the file.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the application does not have permission to write to the specified path.
        /// </exception>
        public virtual void Close()
        {
            FileStream fs;
            StreamWriter sw;
            string settingpath = Localize.PathLocalize(SettingsPath);
            Logger.Info($"{Path} Closing");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingpath)?? "");
            fs = new(settingpath, FileMode.Create);
            sw = new(fs);
            if (Settings != null) sw.Write(Settings.ToString());
            sw.Close();
            fs.Close();
            Logger.Info($"{Path} Closed. File Saved");
        }
    }

    /// <summary>
    /// Different types of Minecraft
    /// Mod:ModLoader like Fabric, Quilt, Liteloader, Forge and NeoForge
    /// Optifine:Only With Optifine
    /// Original:No modloader or Optifine.
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// Original Game
        /// </summary>
        Original,
        /// <summary>
        /// Optifine Game
        /// </summary>
        Optifine,
        /// <summary>
        /// Mod Game
        /// </summary>
        Mod
    }
}
