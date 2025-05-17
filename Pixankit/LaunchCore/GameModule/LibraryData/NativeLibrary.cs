using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.SystemInf;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace PixanKit.LaunchCore.GameModule.LibraryData;

/// <summary>
/// Represents a native library used in the Minecraft environment.
/// </summary>
public class NativeLibrary : LibraryBase
{
    #region Fields
    private string[] _exclude = [];

    /// <summary>
    /// Gets the path of the library.
    /// </summary>
    public override string LibraryPath => "${library_directory}" + Name;
    #endregion

    #region Init
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibrary"/> class for internal use without extraction.
    /// </summary>
    private NativeLibrary()
    {
        LibraryType = LibraryType.Native;
    }
    #endregion

    #region Factory

    /// <summary>
    /// 
    /// </summary>
    /// <param name="libraryJData"></param>
    /// <param name="library"></param>
    /// <returns></returns>
    public static bool CreateInstance(JObject libraryJData, [NotNullWhen(true)]out LibraryBase? library)
    {
        library = null;
        var osKey =
            libraryJData.GetOrDefault(Format.ToString,
                $"natives/{SysInfo.OsName}", "null");
        if (osKey == "null") return false;
            
        var current = libraryJData.GetValue(Format.ToJObject, $"downloads/classifiers/{osKey}");
        var path = current.GetValue(Format.ToString, "path");
        var sha1 = current.GetValue(Format.ToString, "sha1");
        var url = current.GetValue(Format.ToString, "url");
            
        List<string> excludelist = [];
        if (libraryJData.TryGetValue(Format.ToJArray, "extract/exclude", out var array))
        {
            excludelist.AddRange(from token in array ?? [] select token.ToString());
        }

        library = new NativeLibrary()
        {
            Name = path,
            Sha1 = sha1,
            Url = url,
            _exclude = [.. excludelist],
            LibraryType = LibraryType.Native
        };
        return true;
    }
    #endregion
        
    #region Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="libraryPath"></param>
    /// <param name="nativesPath"></param>
    public async Task ExtractAsync(string libraryPath, string nativesPath)
    {
        await using FileStream fs = new(
            LibraryPath.Replace("${library_directory}", libraryPath), 
            FileMode.Open);
        using ZipArchive archive = new(fs);
            
        var tasks = archive.Entries
            .Where(entry => NeedsDecompress(entry.FullName))
            .Select(entry => DecompressAsync(entry, nativesPath));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Determines whether the specified file path should be excluded from extraction.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <returns><c>true</c> if the file should be included; otherwise, <c>false</c>.</returns>
    private bool NeedsDecompress(string fullPath)
    {
        return !fullPath.EndsWith('/') &&
               _exclude.All(path => !fullPath.StartsWith(path));
    }
        
    private async Task DecompressAsync(ZipArchiveEntry entry, string nativesDirPath)
    {
        var fullPath = $"{nativesDirPath}{entry.FullName}";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            if (File.Exists(fullPath)) return;

            await using var entryStream = entry.Open();
            await using FileStream fileStream = 
                new(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await entryStream.CopyToAsync(fileStream);
            Logger.Logger.Info($"Finished decompressing {fullPath}");
        }
        catch (IOException ioEx)
        {
            Logger.Logger.Error($"IO error while decompressing {entry.FullName}: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            Logger.Logger.Error($"Error decompressing {entry.FullName}: {ex.Message}");
        }
    }
    #endregion
}