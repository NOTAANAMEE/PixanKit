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
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using PixanKit.ResourceDownloader.PostProcess;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing the Fabric mod loader for Minecraft.
    /// </summary>
    public class FabricInstaller : SequenceProgressTask
    {
        Folder Owner;

        string Name;

        string version;

        JObject fabricversion;

        string installerpath
        {
            get => $"{Files.CacheDir}/Installer/fabric.jar";
        }

        string fabricversioname = "";

        string url = "";

        FuncProgressTask<int> InitTask = new();

        AsyncProgressTask? DownloadTask;

        CLITask? CommandTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="FabricInstaller"/> class.
        /// </summary>
        /// <param name="folder">The target folder where Minecraft is located.</param>
        /// <param name="name">The name of the Minecraft instance for which Fabric is being installed.</param>
        /// <param name="mcversion">The Minecraft version for which Fabric is being installed.</param>
        /// <param name="fabricversion">A JSON object containing the Fabric version details.</param>
        public FabricInstaller(Folder folder, string name, string mcversion, JObject fabricversion)
        {
            Owner = folder;
            Name = name;
            version = mcversion;
            this.fabricversion = fabricversion;
            Init();
        }

        private void Init()
        {
            InitTask.Function += Init;
            Add(InitTask);
            AddDownloadTask();
            AddCommandTask();
            fabricversioname = $"fabric-loader-{fabricversion["version"]}-{version}";
            this.OnFinish += (a) => { FinishTask(); };
        }

        private void AddDownloadTask()
        {
            DownloadTask = new();
            FileDownloadTask download = new ("", installerpath);
            InitTask.OnFinish += (a) =>
            {
                download.SetURL(url);
            };
            DownloadTask.Add(download);
            if (Owner.FindGame(version) == null)
                DownloadTask.Add(new VanillaMinimalInstallTask(Owner, version, version));
            Add(DownloadTask);
            //DownloadTask.OnFinish += (a) => { Console.WriteLine("DownloadTask Finish"); };
        }

        private void AddCommandTask()
        {
            if (Launcher.Instance == null) 
                throw new InvalidOperationException("Launcher hasn't inited yet");
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes)??
                throw new InvalidOperationException("No java found");
            CommandTask = new(java.JavaEXE, $"-jar \"{installerpath}\" client " +
                $"-dir \"{Owner.FolderPath}\" -mcversion {version} -loader {fabricversion["version"]} " +
                $"\"{Owner.FolderPath}\"");
            Add(CommandTask);
        }

        private void FinishTask()
        {
            GamePostProcess.Move(Owner, fabricversioname, Name);
        }

        private async Task<int> Init(Action<double> progress, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["fabric"]
                    .GetURL(fabricversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1);
            return 0;
        }
    }
}