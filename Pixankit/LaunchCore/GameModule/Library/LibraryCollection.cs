using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using System.IO.Compression;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    public async Task Extract(GameBase game)
    {
        var needToExtract = Libraries
            .Where(l => l.Extract);
        var libPath = game.LibrariesDirPath;
        var nativePath = game.NativeDirPath;
        var extractTasks = 
            needToExtract.Select(library => ExtractLibrary(library, libPath, nativePath))
                .ToList();
        await Task.WhenAll(extractTasks);
        
    }

    private async Task ExtractLibrary(Library library,
        string libPath, string nativePath)
    {
        var zipPath = library.LibraryPath.Replace("${library_path}", libPath);
        if (!File.Exists(zipPath)) return;
        var fs = new FileStream(zipPath, FileMode.Open);
        var archive = new ZipArchive(fs);
        try
        {
            foreach (var entry in archive.Entries)
            {
                var destPath = Path.Combine(nativePath, entry.FullName);
                
                await using var entryStream = entry.Open();
                await using var destStream = new FileStream(
                    destPath, FileMode.Create, FileAccess.Write, 
                    FileShare.None, 4096, useAsync: true);
                await entryStream.CopyToAsync(destStream);
            }
        }
        finally
        {
            archive.Dispose();
            fs.Close();
        }
    }
}