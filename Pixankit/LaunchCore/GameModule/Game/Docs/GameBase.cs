using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Folders;

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
    public virtual partial void Close();
}