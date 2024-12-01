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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    public class QuiltInstaller : SequenceProgressTask
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
            CLITask task = new(java.JavaEXE, $"-jar {installerpath} install client " +
                $"{MCVersion} {quiltversion["version"]} --install-dir {Owner.Path}");
            ProgressTasks.Add(task);
            task.OnFinish += (a) =>
            {
                Owner.AddGame(new ModLoaderGame($"{Owner.VersionDir}/{Name}"));
            };
        }

        private async Task<int> InitTask(Action<double> progress, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["fabric"]
                    .GetURL(quiltversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1.0);
            return 0;
        }
    }
}
