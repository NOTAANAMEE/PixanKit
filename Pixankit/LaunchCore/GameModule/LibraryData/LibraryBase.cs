namespace PixanKit.LaunchCore.GameModule.LibraryData;

/// <summary>
/// Represents the base class for managing libraries in a Minecraft environment.
/// </summary>
/// <remarks>
/// This abstract class defines the structure and behavior of various library types, 
/// such as Vanilla libraries, native libraries, and mod libraries. 
/// It includes functionality for initializing libraries from JSON data, 
/// determining their type and compatibility, and managing their paths and URLs.
/// </remarks>
public abstract class LibraryBase
{
    #region Properties
    /// <summary>
    /// Library Name Like <c>com.ibm.icu:icu4j:73.2</c>
    /// </summary>
    protected string Name { get; init; } = "";
    
    private string LibPath => LibraryHelper.GetPath(Name);

    /// <summary>
    /// The Absolute Path Of The Library
    /// </summary>
    public virtual string LibraryPath  => "${library_directory}" + LibPath;

    /// <summary>
    /// Download URL Of The Library
    /// </summary>
    public string Url { get; protected init; } = "";

    /// <summary>
    /// The Type Of The Library
    /// </summary>
    public LibraryType LibraryType { get; protected init; } = LibraryType.Default;

    /// <summary>
    /// SHA1 Of The Folder
    /// </summary>
    public string Sha1 { get; protected init; } = "";
    #endregion

    #region Initors

    /// <summary>
    /// Constructor
    /// </summary>
    protected LibraryBase() { }
    #endregion
}

/// <summary>
/// Several library types
/// </summary>
public enum LibraryType
{
    /// <summary>
    /// Vanilla Library Type. Just Download
    /// </summary>
    Default,
    /// <summary>
    /// Native Library Type. Need Extract
    /// </summary>
    Native,
    /// <summary>
    /// Mod Loader Generated. Just download
    /// </summary>
    Mod
}

