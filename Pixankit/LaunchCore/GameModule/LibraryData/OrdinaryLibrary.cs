using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.LibraryData;

/// <summary>
/// Represents an Vanilla library in the Minecraft environment.
/// </summary>
public class DefaultLibrary : LibraryBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultLibrary"/> class for internal use.
    /// </summary>
    private DefaultLibrary() : base()
    {
        LibraryType = LibraryData.LibraryType.Default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="libraryJData"></param>
    /// <param name="library"></param>
    /// <returns></returns>
    public static bool CreateInstance(JObject libraryJData, out LibraryBase library)
    {
        library = new DefaultLibrary()
        {
            Url = libraryJData.GetOrDefault(Format.ToString,
                "downloads/artifact/url", ""),
            Sha1 = libraryJData.GetOrDefault(Format.ToString,
                "downloads/artifact/sha1", ""),
            Name = libraryJData.GetOrDefault(Format.ToString,
                "name", ""),
        };
        return true;
    }
        
}