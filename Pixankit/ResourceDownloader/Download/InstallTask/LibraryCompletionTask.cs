using PixanKit.LaunchCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.SystemInf;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using PixanKit.ResourceDownloader.Download;
using PixanKit.LaunchCore.GameModule.Game;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// The Task That Downloads Libraries 
    /// </summary>
    public class LibraryCompletionTask:MultiFileDownload
    {
        /// <summary>
        /// Init A <c>LibraryCompletionTask</c>
        /// </summary>
        public LibraryCompletionTask() : base() { }
        
        /// <summary>
        /// Set The Minecraft Game
        /// </summary>
        /// <param name="game">The GameBase That Needs To Complete</param>
        public void SetMinecraft(GameBase game)
        {
            var libraries = game.GetLibraries();
            foreach (var library in libraries) 
            {
                if (!File.Exists(library.Path))
                    Add(library.Url, library.Path);
            }
        }
    }
}
