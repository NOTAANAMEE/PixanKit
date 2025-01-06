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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using PixanKit.ResourceDownloader.PostProcess;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing the Quilt mod loader for Minecraft.
    /// </summary>
    public class QuiltInstaller : SequenceProgressTask
    {
        readonly Folder Owner;

        readonly string Name;

        readonly string version;

        readonly JObject quiltversion;

        readonly string installerpath = $"{Files.CacheDir}/Installer/quilt.jar";

        string url = "";

        FuncProgressTask<int> InitProgressTask = new();

        AsyncProgressTask? DownloadTask;

        CLITask? CommandTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuiltInstaller"/> class.
        /// </summary>
        /// <param name="folder">The target folder where Minecraft is located.</param>
        /// <param name="name">The name of the Minecraft instance for which Quilt is being installed.</param>
        /// <param name="mcversion">The Minecraft version for which Quilt is being installed.</param>
        /// <param name="quiltversion">A JSON object containing the Quilt version details.</param>
        public QuiltInstaller(Folder folder, string name, string mcversion, JObject quiltversion)
        {
            Owner = folder;
            Name = name;
            version = mcversion;
            this.quiltversion = quiltversion;
            Init();
        }

        private void Init()
        {
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
            if (Owner.FindGame(version) == null)
                DownloadTask.Add(new VanillaMinimalInstallTask(Owner, version, version));
            DownloadTask.Add(download);
            Add(DownloadTask);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance?.JavaRuntimes ?? []) ?? 
                throw new Exception("No Java");
            string workingdir = Path.GetDirectoryName(Owner.FolderPath) ?? "./";
            CommandTask = new(java.JavaEXE, $"-jar \"{Path.GetFullPath(installerpath)}\" " +
                $"install client " +
                $"{version} {quiltversion["version"]} \"--install-dir=./.minecraft\"", workingdir);
            ProgressTasks.Add(CommandTask);
            CommandTask.OnFinish += (a) =>
            {
                GamePostProcess.Move(Owner, 
                    $"quilt-loader-{quiltversion["version"]}-{version}", Name);
            };
        }

        private async Task<int> InitTask(Action<double> progress, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["quilt"]
                    .GetURL(quiltversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1.0);
            return 0;
        }
    }
}
