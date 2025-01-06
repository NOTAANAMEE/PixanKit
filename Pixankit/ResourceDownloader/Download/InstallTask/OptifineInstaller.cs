using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ResourceDownloader.Tasks;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.ModLoaders;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.PostProcess;
using PixanKit.LaunchCore.JavaModule.Java;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing Optifine, a Minecraft optimization mod.
    /// </summary>
    public class OptifineInstaller: SequenceProgressTask
    {
        readonly string version = "";

        readonly Folder Owner;

        readonly string Name;

        static string installerpath { get => $"{Files.CacheDir}/Installer/optifine.jar"; }

        //The Java Program I made myself. It is just used to handle the optifine install task
        static string programpath { get => $"{Files.CacheDir}/Installer/optifineinstaller.jar"; }

        string url = "";

        readonly JObject OptifineVersion;

        FuncProgressTask<int> InitProgressTask = new();

        AsyncProgressTask? DownloadTask;

        CLITask? CommandTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptifineInstaller"/> class.
        /// </summary>
        /// <param name="folder">The target folder where Minecraft is located.</param>
        /// <param name="name">The name of the Minecraft instance. The JAR file will be located at <c>folder\name\name.jar</c>.</param>
        /// <param name="mcversion">The Minecraft version for which Optifine is being installed.</param>
        /// <param name="optifineversion">A JSON object containing the Optifine version details.</param>
        public OptifineInstaller(Folder folder, string name, string mcversion, JObject optifineversion) 
        {
            Name = name;
            Owner = folder;
            version = mcversion;
            OptifineVersion = optifineversion;
            Init(optifineversion);
        }


        private void Init(JObject optifineversion)
        {
            string file = Localize.PathLocalize($"{Files.CacheDir}/Installer/optifine.jar");
            InitProgressTask.Function += GetURL;
            Add(InitProgressTask);
            AddDownloadTask();
            AddCommandTask();
        }

        private void AddDownloadTask()
        {
            DownloadTask = new();
            SimpleFileDownloadTask download = new("", installerpath);
            InitProgressTask.OnFinish += (a) =>
            {
                download.SetURL(url);
            };
            if (Owner.FindGame(version) == null)
            {
                DownloadTask.Add(new VanillaMinimalInstallTask(Owner, version, version));
            }
            DownloadTask.Add(download);
            Add(DownloadTask);
        }

        private void AddCommandTask()
        {
            JavaRuntime? java;
            if (Launcher.Instance == null) throw new InvalidOperationException("Init Launcher first");
            java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            if (java == null) throw new Exception("No java found");
            
            string dir =
                $"{version}-{(OptifineVersion["version"] ?? "").ToString().Replace(" ", "_")}";

            CommandTask = new(java.JavaEXE, "-cp " +
                $"\"{installerpath}{Localize.LocalParser}{programpath}\" Program " +
                $"\"{Owner.FolderPath}\"");
            CommandTask.OnFinish += (a) =>
            {
                GamePostProcess.Move(Owner, dir, Name);
            };
            Add(CommandTask);
        }

        private async Task<int> GetURL(Action<double> report, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["optifine"]
                    .GetURL(OptifineVersion, token);
            return 0;
        }
    }
}
