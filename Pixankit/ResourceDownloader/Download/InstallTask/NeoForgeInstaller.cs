using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using ResourceDownloader.Download.InstallTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing the NeoForge mod loader for Minecraft.
    /// </summary>
    public class NeoForgeInstaller : SequenceProgressTask
    {
        string MCVersion = "";

        Folder Owner;

        string Name;

        string version;

        JObject neoforgeversion;

        string installerpath { get => $"{Files.CacheDir}/Installer/neoforge.jar"; }

        string url = "";

        FuncProgressTask<int> InitProgressTask;

        AsyncProgressTask DownloadTask;

        CLITask CommandTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeoForgeInstaller"/> class.
        /// </summary>
        /// <param name="folder">The folder where NeoForge will be installed.</param>
        /// <param name="name">The name of the NeoForge installation.</param>
        /// <param name="mcversion">The Minecraft version for which NeoForge is being installed.</param>
        /// <param name="neoforgeversion">The JSON object containing the NeoForge version details.</param>
        public NeoForgeInstaller(Folder folder, string name, string mcversion, JObject neoforgeversion)
        {
            Owner = folder;
            Name = name;
            version = mcversion;
            this.neoforgeversion = neoforgeversion;
            Init();
        }

        private void Init()
        {
            InitProgressTask = new();
            InitProgressTask.Function += InitTask;
            Add(InitProgressTask);
            AddDownloadTask();
            AddCommandTask();
        }


        private void AddDownloadTask()
        {
            DownloadTask = new();
            FileDownloadTask download = new("", installerpath);
            InitProgressTask.OnFinish += (a) =>
            {
                download.SetURL(url);
            };
            if (Owner.FindGame(MCVersion) == null)
                DownloadTask.Add(new MinimalOriginalInstallTask(Owner, MCVersion, MCVersion));
            DownloadTask.Add(download);
            Add(DownloadTask);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            CLITask task = new(java.JavaEXE, $"-jar \"{installerpath}\" --installClient " +
                $"\"{Owner.Path}\"");
            ProgressTasks.Add(task);
            task.OnFinish += (a) =>
            {
                ModLoaderServer.Move(Owner, "", Name);
            };
        }

        private async Task<int> InitTask(Action<double> progress, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["neoforge"]
                    .GetURL(neoforgeversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1.0);
            return 0;
        }
    }
}
