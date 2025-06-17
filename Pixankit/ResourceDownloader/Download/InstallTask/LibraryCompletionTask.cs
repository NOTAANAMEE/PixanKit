using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.Param;
using PixanKit.ResourceDownloader.Download.DownloadTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask;

/// <summary>
/// Represents a task for completing the download of necessary libraries for a Minecraft game instance.
/// </summary>
public class LibraryCompletionTask : MultiFileDownloadTask
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryCompletionTask"/> class.
    /// </summary>
    /// <param name="game">The <see cref="GameBase"/> instance representing the Minecraft game for which libraries are being downloaded.</param>
    public LibraryCompletionTask(GameBase game)
    {
        Set(game);
    }

    internal LibraryCompletionTask() { }

    internal void Set(GameBase game)
    {
        List<string> urls = [];
        List<string> files = [];
        var lib = game is CustomizedGame customized?
            ParameterManager.Instance.GetBaseLibrary(customized) :
            ParameterManager.Instance.GetLibrary(game);
        foreach (var library in lib.Libraries)
        {
            var path = library.LibraryPath.Replace("${library_directory}",
                game.LibrariesDirPath);
            if (library.Url == "") continue;
            if (File.Exists(Localize.PathLocalize(path))) continue;
            urls.Add(library.Url);
            files.Add(path);
        }
        Urls = [.. urls];
        Paths = [.. files];
        Init();
    }
}