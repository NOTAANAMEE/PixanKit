using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.ModLoaders;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
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
    public class ForgeInstaller:MultiSequenceTask
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
            TrackActionTask trackFuncTask = new();
            trackFuncTask.Action += InitTask;
            AddMinecraftInstallTask();
            AddDownloadTask();
            AddCommandTask();
        }

        private void AddMinecraftInstallTask()
        {
            if (Owner.FindVersion(MCVersion, LaunchCore.GameModule.Game.GameType.Ordinary) == null)
            Add(new OrdinaryInstallTask(Owner, MCVersion, MCVersion));
        }

        private void AddDownloadTask()
        {
            MultiThreadDownload download = new(installerpath);
            var process = processes[0] as TrackFuncTask<string>;
            process.OnFinish += () =>
            {
                download.SetURL(process.Return);
            };
            Add(download);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            CLIRunningTask task = new(java.JavaEXE, $"-jar {installerpath} --installClient " +
                $"{Owner.Path}");
            processes.Add(task);
            task.OnFinish += () =>
            {
                Owner.AddGame(new ModloaderGame($"{Owner.VersionDir}/{Name}"));
            };
        }

        private async Task InitTask(TrackActionTask task, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["forge"]
                    .GetURL(forgeversion, token);
            if (token.IsCancellationRequested) return;
            task.Sched = 10;
        }
    }
}
