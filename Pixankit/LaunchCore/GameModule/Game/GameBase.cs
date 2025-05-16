using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.GameModule.Folders;

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
        #region Properties
        /// <summary>
        /// Gets the name of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// This is derived from the folder name containing the game files.
        /// </remarks>
        public string Name => _name; 

        /// <summary>
        /// Gets or sets the description of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// Default value is "Minecraft" if not specified.
        /// </remarks>
        public string Description
        {
            get => (Settings["description"] ?? "Minecraft").ToString();
            set => Settings["description"] = value;
        }

        /// <summary>
        /// Gets the folder where the main game files (e.g., game.jar) are located.
        /// </summary>
        public string GameFolderPath => $"{_folder.VersionDirPath}{_name}/"; 

        /// <summary>
        /// Gets the owner folder of the Minecraft game.
        /// </summary>
        /// <remarks>
        /// This represents the parent directory of the game folder.
        /// </remarks>
        public Folder Owner => _folder; 

        /// <summary>
        /// Gets the full path to the game JAR file.
        /// </summary>
        /// <remarks>
        /// Combines the <see cref="GameFolderPath"/> and the <see cref="Name"/> to locate the JAR file.
        /// </remarks>
        public string GameJarFilePath => $"{GameFolderPath}{Name}.jar"; 

        /// <summary>
        /// Gets the full path to the game's JSON configuration file.
        /// </summary>
        /// <remarks>
        /// This file stores metadata about the game instance.
        /// </remarks>
        public string GameJsonFilePath => $"{GameFolderPath}{Name}.json"; 
        
        /// <summary>
        /// Gets the folder where library files are stored.
        /// </summary>
        /// <remarks>
        /// Defaults to the global libraries folder if no specific owner folder is defined.
        /// </remarks>
        public string LibrariesDirPath => _folder.LibraryDirPath;

        /// <summary>
        /// Gets the folder where texture packs and assets are stored.
        /// </summary>
        /// <remarks>
        /// Defaults to the global assets folder derived from the Minecraft directory structure.
        /// </remarks>
        public string AssetsDirPath => _folder.AssetsDirPath;

        /// <summary>
        /// Gets the root folder of the Minecraft installation.
        /// </summary>
        /// <remarks>
        /// This is the base directory containing all versions and global assets.
        /// </remarks>
        public string GameRootFolderPath => _folder.FolderPath;

        /// <summary>
        /// Gets the Minecraft version for this game instance.
        /// </summary>
        public string Version => Params.Version;

        /// <summary>
        /// Gets the folder where native binary libraries are stored.
        /// </summary>
        /// <remarks>
        /// These binaries are required for platform-specific functionality.
        /// </remarks>
        public string NativeDirPath => $"{GameFolderPath}{Name}-natives/";

        /// <summary>
        /// Gets the path to the settings configuration file.
        /// </summary>
        /// <remarks>
        /// The settings file contains user and game-specific configurations.
        /// </remarks>
        public string SettingsPath => GameFolderPath + Files.SettingsPath; 

        /// <summary>
        /// Gets the type of the game instance.
        /// </summary>
        /// <remarks>
        /// The game type defines whether the instance is a vanilla, modded, or other variant.
        /// </remarks>
        public GameType GameType { get; protected set; } = GameType.Vanilla;

        /// <summary>
        /// Gets the settings for this game instance.
        /// </summary>
        /// <remarks>
        /// The settings define Java version preferences, runtime folder behavior, and custom descriptions.
        /// </remarks>
        public JObject Settings = new()
        {
            { "java", "overall"},
            { "custom_java", "" },
            { "jvm_argument", "overall" },
            { "custom_jvm_argument", "" },
            { "running_folder", "overall" },
            { "custom_running_folder", "" },
            { "description", "A Minecraft Game" },
            { "pre_argument", "overall" },
            { "custom_pre_argument", "" },
            { "post_argument", "overall" },
            { "custom_post_argument", "" },
            { "env_variables", "overall" },
            { "custom_env_variables", "" },
        };

        /// <summary>
        /// Gets the minimal java version for choosing the java runtime
        /// </summary>
        public short MinimalJavaVersion => Params.JvmVersion;
        #endregion

        #region Fields
        /// <summary>
        /// The parameters of the game instance.
        /// </summary>
        protected GameParameter Params;

        private readonly Folder _folder;

        private readonly string _name;

        /// <summary>
        /// 
        /// </summary>
        protected string ReleaseType = "release";

        /// <summary>
        /// The reference of library
        /// </summary>
        protected LibrariesRef LibrariesRef;
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
                Settings = JObject.Parse(
                    File.ReadAllText(SettingsPath));
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
        public LibraryBase[] GetLibraries()
        {
            return LibrariesRef.Libraries;
        }

        /// <summary>
        /// Initializes the game instance with the provided JSON configuration.
        /// </summary>
        /// <param name="gameJdata">A JSON object containing the game configuration.</param>
        /// <remarks>
        /// This method allows convenient subclass initialization by loading configuration details 
        /// such as assets, libraries, and runtime settings from a JSON file.
        /// </remarks>
        protected virtual void LoadJson(JObject gameJdata)
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
            string settingPath = SettingsPath;
            string directoryPath = Path.GetDirectoryName(settingPath) ?? "";

            Logger.Logger.Info($"{GameJarFilePath} Closing");

            if (directoryPath.Length != 0 && !Directory.Exists(directoryPath)) 
                Directory.CreateDirectory(directoryPath);
            
            using FileStream fs = new(settingPath, FileMode.Create);
            using StreamWriter sw = new(fs);
            sw.Write(Settings.ToString());

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
}
