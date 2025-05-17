using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.LibraryData;

/// <summary>
/// 
/// </summary>
public class LibrariesRef
{
    /// <summary>
    /// 
    /// </summary>
    public string Version { get; }
        
    /// <summary>
    /// 
    /// </summary>
    public LibraryBase[] Libraries => [.._libraries];
        
    private readonly List<LibraryBase> _libraries = new();
        
    private readonly List<NativeLibrary> nativeLibraries = [];

    private LibrariesRef(string version, JObject jData)
    {
        Version = version;
        var array = jData.GetOrDefault(Format.ToJArray, "libraries", []);
        foreach (var token in array)
        {
            LibraryHelper.AddLibrary(
                token.ConvertTo(Format.ToJObject, []), _libraries);
            var last = _libraries.LastOrDefault();
            if (last != null && last is NativeLibrary lib) nativeLibraries.Add(lib);
        }
        Logger.Logger.Info($"Libraries Added. Number:{_libraries.Count}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="libraryPath"></param>
    /// <param name="nativePath"></param>
    public async Task Extract(string libraryPath, string nativePath)
    {
        var tasks = nativeLibraries.Select(
            lib => lib.ExtractAsync(libraryPath, nativePath));
        await Task.WhenAll(tasks);
    }
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    /// <param name="jData"></param>
    /// <returns></returns>
    public static LibrariesRef CreateInstance(string version, JObject jData)
        => new LibrariesRef(version, jData);
}