using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.LibraryData;

/// <summary>
/// Represents a library specific to mod loaders.
/// </summary>
public class LoaderLibrary : LibraryBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoaderLibrary"/> class for internal use.
    /// </summary>
    private LoaderLibrary() : base()
    {
        LibraryType = LibraryData.LibraryType.Mod;
    }
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="jData"></param>
    /// <param name="library"></param>
    public static void CreateInstance(JObject jData, out LibraryBase library)
    {
        library = new LoaderLibrary()
        {
            Url = jData.GetOrDefault(Format.ToString,"url", ""),
            Name = jData.GetOrDefault(Format.ToString, "name", ""),
        };
    }
}