using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Log;

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
    public abstract partial class GameBase
    {
        /// <summary>
        /// Represents the game argument that needs conditions to activate. 
        /// It is optional as these parameters will not affect the normal 
        /// operation of the program
        /// </summary>
        /// <param name="arg">The argument itself</param>
        /// <param name="rules">
        /// The rules that activate the argument<br/>
        /// The rule string matches the format<br/>
        /// <c>arch=x86</c><br/>
        /// <c>os=!osx</c>
        /// </param>
        protected record class OptionalArgs(string arg, string[] rules)
        {
            /// <summary>
            /// Parses the JObject to an OptionalArgs instance
            /// </summary>
            /// <param name="jData"></param>
            /// <returns>the argument instance</returns>
            public static OptionalArgs Parse(JObject jData)
            {
                List<string> rules = [];
                rules.AddRange(from rule in jData[nameof(rules)]
                               select ParseRule((JObject)rule));
                return new OptionalArgs(ParseArg(jData), [.. rules]);
            }

            private static string ParseArg(JObject jData)
            {
                string ret = "";
                if (jData["value"] == null) throw new();
                if (jData["value"]?.Type == JTokenType.String) 
                    return (jData["value"] ?? "").ToString();
                foreach (var token in jData["value"] ?? new JArray())
                {
                    ret += $"{token} ";
                }
                return ret;
            }

            private static string ParseRule(JObject jData)
            {
                string sign = Allow(jData) ? "" : "!";

                if (jData["features"] is JObject features)
                {
                    foreach (var rule in features)
                    {
                        return $"{rule.Key}=" +
                               $"{sign}" +
                               $"{rule.Value}";
                    }
                }

                return string.Empty;
            }

            private static bool Allow(JObject jData)
                => jData["action"]?.ToString() == "allow";
        }

        #region Properties
        /// <summary>
        /// Gets the name of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// This is derived from the folder name containing the game files.
        /// </remarks>
        public string Name { get => Path.GetFileName(_gameFolderPath) ?? ""; }

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
        public string GameFolderPath { get => _gameFolderPath; }

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
        /// Combines the <see cref="GameFolderPath"/> and the <see cref="Name"/> to locate the JAR file.
        /// </remarks>
        public string GameJarFilePath { get => _gameFolderPath + $"/{Name}.jar"; }

        /// <summary>
        /// Gets the full path to the game's JSON configuration file.
        /// </summary>
        /// <remarks>
        /// This file stores metadata about the game instance.
        /// </remarks>
        public string GameJsonFilePath { get => _gameFolderPath + $"/{Name}.json"; }

        /// <summary>
        /// Gets or sets the folder used to store world data, mods, and other runtime files.
        /// </summary>
        /// <remarks>
        /// Also known as the "game directory".
        /// </remarks>
        public string GameRunningDirPath
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
        public string LibrariesDirPath
        {
            get => (folder == null)? GameRootFolderPath + 
                "/libraries":folder.LibraryDirPath;
        }

        /// <summary>
        /// Gets the folder where texture packs and assets are stored.
        /// </summary>
        /// <remarks>
        /// Defaults to the global assets folder derived from the Minecraft directory structure.
        /// </remarks>
        public string AssetsDirPath 
        { 
            get => (folder == null)? _gameFolderPath.Remove(_gameFolderPath.LastIndexOf("/versions/")) + "/assets"
                :folder.AssetsDirPath; 
        }

        /// <summary>
        /// Gets the root folder of the Minecraft installation.
        /// </summary>
        /// <remarks>
        /// This is the base directory containing all versions and global assets.
        /// </remarks>
        public string GameRootFolderPath{ get => 
        (folder == null)? _gameFolderPath.Remove(_gameFolderPath.LastIndexOf("/versions/")):
                folder.FolderPath; }

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
        public string NativeDirPath { get => _gameFolderPath + $"/{Name}-natives"; }

        /// <summary>
        /// Gets the path to the settings configuration file.
        /// </summary>
        /// <remarks>
        /// The settings file contains user and game-specific configurations.
        /// </remarks>
        public string SettingsPath { get => _gameFolderPath + Files.SettingsPath; }

        /// <summary>
        /// Gets the folder where crash reports are stored.
        /// </summary>
        /// <remarks>
        /// Crash reports are saved as compressed tar.gz files in this directory.
        /// </remarks>
        public string CrashReportDirPath { get => GameRunningDirPath + "/crash-reports"; }

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

        /// <summary>
        /// Gets the minimal java version for choosing the java runtime
        /// </summary>
        public short MinimalJavaVersion { get => javaVersion; }
        #endregion

        #region Fields
        /// <summary>
        /// Represents a temporary JSON data of the Minecraft version
        /// </summary>
        protected JObject gameJSONData = [];

        /// <summary>
        /// Represents the folder of the game
        /// </summary>
        protected Folder? folder = null;

        /// <summary>
        /// Represents the path of the directory which contains the game jar file 
        /// </summary>
        protected string _gameFolderPath = "";

        /// <summary>
        /// Represents the version name of the game
        /// </summary>
        protected string _version = "";

        /// <summary>
        /// Represents the 
        /// <see cref="PixanKit.LaunchCore.GameModule.Game.GameType"/> 
        /// of the game
        /// </summary>
        protected GameType _gameType;

        /// <summary>
        /// Represents the collection of the libraries
        /// </summary>
        protected List<LibraryBase> libraries = [];

        /// <summary>
        /// Represents the main class name of the jar file
        /// </summary>
        protected string className = "";

        /// <summary>
        /// Represents the game arguments that required to launch
        /// </summary>
        protected string gameArguments = "";

        /// <summary>
        /// Represents the java arguments that required to launch Minecraft
        /// </summary>
        protected string javaArguments = "";

        /// <summary>
        /// Represents the minimal version of Java
        /// </summary>
        protected short javaVersion = 8;

        /// <summary>
        /// Represents the id of the assetsindex file
        /// </summary>
        protected string assetsID = "";

        internal string releaseType = "";

        /// <summary>
        /// Represents the game args which is optional
        /// </summary>
        protected List<OptionalArgs> optionalArgs = [];
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
            Init();
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
            gameJSONData = jData;
            Init();
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
            _gameFolderPath = path;
            if (initFromFile)
            {
                gameJSONData = ReadJObj($"{path}/{Name}.json");
            }
            Logger.Info($"Game Base Added. Path:{_gameFolderPath}");
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
        /// Init the game
        /// </summary>
        public void Init()
        {
            SetMainClass();
            SetJVMArgs();
            SetGameArgs();
            SetVersion();
            SetLibrary();
            releaseType = (gameJSONData["type"] ?? "release").ToString();
            LoadJSON(gameJSONData);
            SetSettings();
            gameJSONData = [];
        }

        /// <summary>
        /// Set the folder of the game
        /// </summary>
        /// <param name="owner">folder. Should be the actual folder that it is in</param>
        public virtual void SetOwner(Folder owner)
        {
            if (_gameFolderPath.StartsWith(owner.FolderPath))
            folder = owner;
        }

        private void SetSettings()
        {
            if (File.Exists(SettingsPath))
                Settings = JObject.Parse(
                    File.ReadAllText(SettingsPath));
        }

        private void SetLibrary()
        {
            var array = gameJSONData.GetOrDefault(Format.ToJArray, "libraries", []);
            foreach (JToken token in array)
            {
                LibraryBase.Parse(token.ConvertTo(Format.ToJObject, []), libraries);
            }
            Logger.Info($"Libraries Added. Number:{libraries.Count}");
        }

        private static JObject ReadJObj(string path)
            => JSON.ReadFromFile(path);

        private void SetVersion()
        {
            if (gameJSONData["inheritsfrom"] != null) 
                _version = (gameJSONData["inheritsfrom"] ?? "").ToString();
            else if (gameJSONData["clientVersion"] != null)
                _version = (gameJSONData["clientVersion"] ?? "").ToString();
            else
                _version = (gameJSONData["id"] ?? "").ToString();
            releaseType = (gameJSONData["type"] ?? "release").ToString();
        }

        #region ArgsParser
        internal virtual void SetGameArgs()
        {
            if (gameJSONData.ContainsKey("minecraftArguments"))
            {
                gameArguments = gameJSONData["minecraftArguments"]?.ToString() ?? "";
                return;
            }
            foreach (JToken token in gameJSONData["arguments"]?["game"] ?? new JArray())
            {
                if (token.Type != JTokenType.String)
                {
                    optionalArgs.Add(OptionalArgs.Parse(token as JObject ?? []));
                    continue;
                }
                gameArguments += token.ToString() + " ";
            }
        }

        internal virtual void SetJVMArgs()
        {
            var jvmargArray = GetJVMArgArray();
            if (jvmargArray == null) return;
            foreach (JToken token in jvmargArray)
            {
                string arg = ParseArg(token);
                if (arg == "") continue;
                javaArguments += $"{arg} ";
            }
        }

        private JArray GetJVMArgArray()
        {
            string defaultJson = 
                "[{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"osx\"}}],\"value\": [\"-XstartOnFirstThread\"]},{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"windows\"}}],\"value\": \"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump\"},{\"rules\": [{\"action\": \"allow\",\"os\": {\"arch\": \"x86\"}}],\"value\": \"-Xss1M\"},\"-Djava.library.path=${natives_directory}\"" +
                ",\"-Dminecraft.launcher.brand=${launcher_name}\",\"-Dminecraft.launcher.version=${launcher_version}\",\"-cp\",\"${classpath}\"]";
            if (gameJSONData.TryGetValue(Format.ToJArray, 
                "arguments/jvm", out JArray? array))
                return array ?? [];
            else
                return JArray.Parse(defaultJson);
        }

        private static string ParseArg(JToken token)
        {
            var arg = "";
            if (token.Type == JTokenType.String)
            {
                arg = token.ToString();
                if (arg.Contains(' ')) arg = "\"" + arg + "\"";
                //" " is needed while space exists
            }
            else if (LibraryBase.SystemSupport((JObject)token))
            {
                var value = token.ConvertTo(Format.ToJObject, [])
                    .GetFromPathCheck("value");
                if (value.Type == JTokenType.String) 
                    return value.ToString();
                arg = string.Join(" ", value.ConvertTo(Format.ToJArray, []));
            }
            return arg;
        }
        #endregion

        private void SetMainClass()
            => className = (gameJSONData["mainClass"] ?? "net.minecraft.client.main.Main.").ToString();
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
            return [..libraries];
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
            var target = Owner?.FindVersion(_version, GameType.Vanilla);
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
            string settingpath = SettingsPath;
            Logger.Info($"{GameJarFilePath} Closing");
            Directory.CreateDirectory(Path.GetDirectoryName(settingpath)?? "");
            fs = new(settingpath, FileMode.Create);
            sw = new(fs);
            if (Settings != null) sw.Write(Settings.ToString());
            sw.Close();
            fs.Close();
            Logger.Info($"{GameJarFilePath} Closed. File Saved");
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
}
