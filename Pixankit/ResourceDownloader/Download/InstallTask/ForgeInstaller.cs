using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Download.ModLoaders;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Forge Installer
    /// </summary>
    public class ForgeInstaller:SequenceProgressTask
    {
        string MCVersion = "";

        Folder Owner;

        string Name;

        string version;

        JObject forgeversion;

        string installerpath = $"{Files.CacheDir}/Installer/forge.jar";

        string url = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="mcversion"></param>
        /// <param name="forgeversion"></param>
        public ForgeInstaller(Folder folder, string name, string mcversion, JObject forgeversion)
        {
            Owner = folder;
            Name = name;
            version = mcversion;
            this.forgeversion = forgeversion;
            
        }

        private void Init()
        {
            FuncProgressTask<int> trackFuncTask = new();
            trackFuncTask.Function += InitTask;
            AddMinecraftInstallTask();
            AddDownloadTask();
            AddCommandTask();
        }

        private void AddMinecraftInstallTask()
        {
            if (Owner.FindVersion(MCVersion, LaunchCore.GameModule.Game.GameType.Original) == null)
            Add(new OriginalInstallTask(Owner, MCVersion, MCVersion));
        }

        private void AddDownloadTask()
        {
            FileDownloadTask download = new("", installerpath);
            var process = ProgressTasks[0] as FuncProgressTask<string>;
            process.OnFinish += (a) =>
            {
                download.SetURL(process.Return);
            };
            Add(download);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            CLITask task = new(java.JavaEXE, $"-jar {installerpath} --installClient " +
                $"{Owner.Path}");
            ProgressTasks.Add(task);
            task.OnFinish += (a) =>
            {
                Owner.AddGame(new ModLoaderGame($"{Owner.VersionDir}/{Name}"));
            };
        }

        private async Task<int> InitTask(Action<double> progress, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["forge"]
                    .GetURL(forgeversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1.0);
            return 0;
        }
    }
}
