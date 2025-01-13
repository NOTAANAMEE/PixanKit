using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.SystemInf;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for completing the download of necessary libraries for a Minecraft game instance.
    /// </summary>
    public class LibraryCompletionTask:MultiFileDownloadTask
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
            foreach (var library in game.GetLibraries())
            {
                string libpath = library.LibraryPath.Replace("${library_directory}",
                    game.LibrariesDirPath);
                if (library.LibraryType == LibraryType.Mod) continue;
                if (File.Exists(Localize.PathLocalize(libpath))) continue;
                urls.Add(library.Url);
                files.Add(libpath);
            }
            this.urls = [.. urls];
            paths = [.. files];
            Init();
        }
    }
}
