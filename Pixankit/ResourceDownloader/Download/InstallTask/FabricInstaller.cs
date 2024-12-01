using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.Download;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    public class FabricInstaller : SequenceProgressTask
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
            FuncProgressTask<int> trackFuncTask = new();
            trackFuncTask.Function += InitTask;
            AddMinecraftInstallTask();
            AddDownloadTask();
            AddCommandTask();
        }

        private void AddMinecraftInstallTask()
        {
            if (Owner.FindVersion(MCVersion, GameType.Original) == null)
                Add(new OriginalInstallTask(Owner, MCVersion, MCVersion));
        }

        private void AddDownloadTask()
        {
            FileDownloadTask download = new ("", installerpath);
            var process = ProgressTasks[0] as FuncProgressTask<string>;
            process.OnFinish += (a) =>
            {
                download.SetURL(url);
            };
            Add(download);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            CLITask task = new(java.JavaEXE, $"-jar {installerpath} client " +
                $"-dir {Owner.Path} -mcversion {MCVersion} -loader {fabricversion["version"]}" +
                $"{Owner.Path}");
            ProgressTasks.Add(task);
            task.OnFinish += (a) =>
            {
                Owner.AddGame(new ModLoaderGame($"{Owner.VersionDir}/{Name}"));
            };
        }

        private async Task<int> InitTask(Action<double> progress, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["fabric"]
                    .GetURL(fabricversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1);
            return 0;
        }
    }
}
