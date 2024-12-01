using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    public class LibraryCompletionTask:MultiFileDownloadTask
    {
        public LibraryCompletionTask(GameBase game)
        {
            Set(game);
        }

        internal LibraryCompletionTask() { }

        internal void Set(GameBase game)
        {
            List<string> urls = new();
            List<string> paths = new();
            foreach (var library in game.GetLibraries())
            {
                if (library.LibraryType == LibraryType.Mod) continue;
                urls.Add(library.Url);
                paths.Add(library.Path);
            }
            Init();
        }
    }
}
