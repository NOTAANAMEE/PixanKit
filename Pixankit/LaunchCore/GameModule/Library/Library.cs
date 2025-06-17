using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.SystemInf;
using System.Diagnostics.CodeAnalysis;

namespace PixanKit.LaunchCore.GameModule.Library;

/// <summary>
/// 
/// </summary>
public class Library
{
    /// <summary>
    /// The Absolute Path Of The Library
    /// </summary>
    public string LibraryPath => $"${{library_directory}}/{GetPath()}.jar";
    
    /// <summary>
    /// Get the SHA1 of the library
    /// </summary>
    public string Sha1 { get; private set; } = "";

    /// <summary>
    /// Get the url address of the library
    /// </summary>
    public string Url { get; private set; } = "";

    /// <summary>
    /// Whether the library is a native library or not
    /// </summary>
    public bool Extract => nativeName != null;

    private string libName = "";

    private string? nativeName;
    
    private Library(){}

    private string GetPath()
    {
        var parts = libName.Split(':');
        var package = parts[0].Replace('.', '/');
        var name = parts[1];
        var version = parts[2];
        var native = nativeName;
        if (native == null && parts.Length > 3) 
            native = parts[3];
        return native != null ? 
            $"{package}/{name}/{name}-{version}-{native}" : 
            $"{parts}/{name}/{name}-{version}";
    }
    
    /// <summary>
    /// factory method to create a Library instance from JSON data
    /// </summary>
    /// <param name="libraryData"></param>
    /// <param name="library"></param>
    /// <returns></returns>
    public static bool Parse(JObject libraryData, 
        [NotNullWhen(true)] out Library? library)
    {
        library = null;
        if (!LibraryHelper.SystemSupport(libraryData))return false;
        if (libraryData.TryGetValue(Format.ToString, "natives", out _)) return 
            NativeParse(libraryData, out library);
        return libraryData.TryGetValue(Format.ToString, "downloads", out _) ? 
            OrdParse(libraryData, out library) : 
            ModParse(libraryData, out library);
    }
    
    
    private static bool NativeParse(JObject libraryData, 
        [NotNullWhen(true)] out Library? lib)
    {
        lib = null;
        if (!libraryData.TryGetValue(Format.ToString,
                $"natives/{SysInfo.OsName}", out var nativeName))
            return false;
        lib = new Library()
        {
            libName = libraryData.GetOrDefault(Format.ToString, "name", ""),
            Url = libraryData.GetOrDefault(Format.ToString, 
                $"downloads/classifiers/{nativeName}/url", ""),
            Sha1 = libraryData.GetOrDefault(Format.ToString, 
                $"downloads/classifiers/{nativeName}/sha1", ""),
            nativeName = nativeName
        };
        return true;
    }
    
    private static bool ModParse(JObject libraryData, [NotNullWhen(true)] out Library? lib)
    {
        
        lib = new Library()
        {
            libName = 
                libraryData.GetOrDefault(Format.ToString, 
                "name", ""),
            Url = 
                libraryData.GetOrDefault(Format.ToString, 
                "url", ""),
            Sha1 = 
                libraryData.GetOrDefault(Format.ToString, 
                "sha1", "")
        };
        return true;
    }
    
    private static bool OrdParse(JObject libraryData, 
        [NotNullWhen(true)] out Library? lib)
    {
        lib = new Library()
        {
            libName = 
                libraryData.GetOrDefault(Format.ToString, 
                "downloads/artifact/name", ""),
            Url = 
                libraryData.GetOrDefault(Format.ToString, 
                "downloads/artifact/url", ""),
            Sha1 = 
                libraryData.GetOrDefault(Format.ToString, 
                "downloads/artifact/sha1", "")
        };
        return true;
    }
}