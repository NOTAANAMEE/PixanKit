using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.Library;

/// <summary>
/// 
/// </summary>
public class LibraryCollection
{
    /// <summary>
    /// The file path of the JSON file containing the libraries
    /// </summary>
    public string FilePath { get; }
    
    /// <summary>
    /// A collection of libraries
    /// </summary>
    public List<Library> Libraries { get; } = [];
    
    /// <summary>
    /// Initialize a new instance of LibraryCollection
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="load"></param>
    public LibraryCollection(string filePath, bool load = false)
    {
        FilePath = filePath;
        if (load) Load();
    }

    /// <summary>
    /// clear the library cache
    /// </summary>
    public void Clear() => Libraries.Clear();

    /// <summary>
    /// load libraries from the specified file path
    /// </summary>
    public void Load()
    {
        var jData = Json.Json.ReadFromFile(FilePath);
        var librariesArray = jData.GetOrDefault(Format.ToJArray, "libraries", []);
        foreach (var libraryData in librariesArray)
        {
            if (Library.Parse(
                    libraryData.ConvertTo(Format.ToJObject, []), 
                    out var library)) 
                Libraries.Add(library);
        }
    }
}