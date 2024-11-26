using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.Download;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.Game;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    public class FabricInstaller : MultiSequenceTask
    {

        string MCVersion = "";

        Folder Owner;

        string Name;

        string version;

        JObject fabricversion;

        string installerpath = $"{Files.CacheDir}/Installer/fabric.jar";

        string url = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="mcversion"></param>
        /// <param name="fabricversion"></param>
        public FabricInstaller(Folder folder, string name, string mcversion, JObject fabricversion)
        {
            Owner = folder;
            Name = name;
            version = mcversion;
            this.fabricversion = fabricversion;

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
            CLIRunningTask task = new(java.JavaEXE, $"-jar {installerpath} client " +
                $"-dir {Owner.Path} -mcversion {MCVersion} -loader {fabricversion["version"]}" +
                $"{Owner.Path}");
            processes.Add(task);
            task.OnFinish += () =>
            {
                Owner.AddGame(new ModloaderGame($"{Owner.VersionDir}/{Name}"));
            };
        }

        private async Task InitTask(TrackActionTask task, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["fabric"]
                    .GetURL(fabricversion, token);
            if (token.IsCancellationRequested) return;
            task.Sched = 10;
        }
    }
}
