using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.GameModule.LibraryData;

// ReSharper disable once CheckNamespace
namespace PixanKit.LaunchCore.GameModule.Game;

public abstract partial class GameBase
{
    #region Properties
    /// <summary>
    /// Gets the name of the Minecraft game.
    /// </summary>
    /// <remarks>
    /// This is derived from the folder name containing the game files.
    /// </remarks>
    public partial string Name { get; }
    
    /// <summary>
    /// Gets or sets the description of the Minecraft game.
    /// </summary>
    /// <remarks>
    /// Default value is "Minecraft" if not specified.
    /// </remarks>
    public partial string Description { get; set; }
    
    /// <summary>
    /// Gets the folder where the main game files (e.g., game.jar) are located.
    /// </summary>
    public partial string GameFolderPath { get; }
    
    /// <summary>
    /// Gets the owner folder of the Minecraft game.
    /// </summary>
    /// <remarks>
    /// This represents the parent directory of the game folder.
    /// </remarks>
    public partial Folder Owner { get; }
    
    /// <summary>
    /// Gets the full path to the game JAR file.
    /// </summary>
    /// <remarks>
    /// Combines the <see cref="GameFolderPath"/> and the <see cref="Name"/> to locate the JAR file.
    /// </remarks>
    public partial string GameJarFilePath { get; }
    
    /// <summary>
    /// Gets the full path to the game's JSON configuration file.
    /// </summary>
    /// <remarks>
    /// This file stores metadata about the game instance.
    /// </remarks>
    public partial string GameJsonFilePath { get; }
    
    /// <summary>
    /// Gets the folder where library files are stored.
    /// </summary>
    /// <remarks>
    /// Defaults to the global libraries folder if no specific owner folder is defined.
    /// </remarks>
    public partial string LibrariesDirPath { get; }
    
    /// <summary>
    /// Gets the folder where texture packs and assets are stored.
    /// </summary>
    /// <remarks>
    /// Defaults to the global assets folder derived from the Minecraft directory structure.
    /// </remarks>
    public partial string AssetsDirPath { get; }
    
    /// <summary>
    /// Gets the root folder of the Minecraft installation.
    /// </summary>
    /// <remarks>
    /// This is the base directory containing all versions and global assets.
    /// </remarks>
    public partial string GameRootFolderPath { get; }
    
    /// <summary>
    /// Gets the Minecraft version for this game instance.
    /// </summary>
    public partial string Version { get; }
    
    /// <summary>
    /// Gets the type of the game instance.
    /// </summary>
    /// <remarks>
    /// The game type defines whether the instance is a vanilla, modded, or other variant.
    /// </remarks>
    public partial GameType GameType { get; protected set; } 
    
    /// <summary>
    /// Gets the folder where native binary libraries are stored.
    /// </summary>
    /// <remarks>
    /// These binaries are required for platform-specific functionality.
    /// </remarks>
    public partial string NativeDirPath { get; }
    
    /// <summary>
    /// Gets the path to the settings configuration file.
    /// </summary>
    /// <remarks>
    /// The settings file contains user and game-specific configurations.
    /// </remarks>
    public partial string SettingsPath { get; }
    
    /// <summary>
    /// Gets the minimal java version for choosing the java runtime
    /// </summary>
    public partial short MinimalJavaVersion { get; }
    #endregion
    
    /// <summary>
    /// Retrieves all libraries associated with this game instance.
    /// </summary>
    /// <returns>An array of <see cref="LibraryBase"/> objects representing the libraries.</returns>
    /// <remarks>
    /// This method returns the complete list of libraries required for the game, including runtime and mod libraries.
    /// </remarks>
    public partial LibraryBase[] GetLibraries();

    /// <summary>
    /// Generates the launch arguments for the game.
    /// </summary>
    /// <returns>A formatted string containing the full launch arguments.</returns>
    /// <remarks>
    /// This method replaces placeholders like <c>${natives_directory}</c> with actual paths
    /// and applies localization. It also incorporates JVM and game-specific arguments.
    /// </remarks>
    public partial string GetLaunchArgument();

    /// <summary>
    /// Decompresses native libraries required for the game.
    /// </summary>
    /// <remarks>
    /// This method extracts all native libraries to the <c>NativeDir</c> folder.
    /// It skips non-native libraries.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public partial Task Decompress();

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
    public virtual partial void Close();

    /// <summary>
    /// Initializes the game instance with the provided JSON configuration.
    /// </summary>
    /// <param name="gameData">A JSON object containing the game configuration.</param>
    /// <remarks>
    /// This method allows convenient subclass initialization by loading configuration details 
    /// such as assets, libraries, and runtime settings from a JSON file.
    /// </remarks>
    protected virtual partial void LoadJson(JObject gameData);

    /// <summary>
    /// Retrieves the classpath arguments for the game.
    /// </summary>
    /// <returns>
    /// A string containing the classpath arguments, excluding the game JAR file.
    /// </returns>
    /// <remarks>
    /// The classpath includes library paths and the main game JAR path.
    /// </remarks>
    protected virtual partial string GetCpArgs();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual partial string GetArgument();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual partial string GetAssetsId();

    /// <summary>
    /// Checks whether the game is ready to launch.
    /// </summary>
    /// <returns><c>true</c> if the game can launch; otherwise, <c>false</c>.</returns>
    public virtual partial bool LaunchCheck();
}