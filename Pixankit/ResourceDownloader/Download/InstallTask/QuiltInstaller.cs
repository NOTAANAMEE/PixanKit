using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.Download;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceDownloader.Download.InstallTask
{
    public class QuiltInstaller : MultiSequenceTask
    {

        string MCVersion = "";

        Folder Owner;

        string Name;

        string version;

        JObject quiltversion;

        string installerpath = $"{Files.CacheDir}/Installer/quilt.jar";

        string url = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="mcversion"></param>
        /// <param name="quiltversion"></param>
        public QuiltInstaller(Folder folder, string name, string mcversion, JObject quiltversion)
        {
            Owner = folder;
            Name = name;
            version = mcversion;
            this.quiltversion = quiltversion;

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
            if (Owner.FindVersion(MCVersion, GameType.Ordinary) == null)
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
            CLIRunningTask task = new(java.JavaEXE, $"-jar {installerpath} install client " +
                $"{MCVersion} {quiltversion["version"]} --install-dir {Owner.Path}");
            processes.Add(task);
            task.OnFinish += () =>
            {
                Owner.AddGame(new ModloaderGame($"{Owner.VersionDir}/{Name}"));
            };
        }

        private async Task InitTask(TrackActionTask task, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["fabric"]
                    .GetURL(quiltversion, token);
            if (token.IsCancellationRequested) return;
            task.Sched = 10;
        }
    }
}
