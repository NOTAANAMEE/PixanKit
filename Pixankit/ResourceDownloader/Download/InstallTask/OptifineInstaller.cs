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
using System.IO.Compression;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;
using ResourceDownloader.Download.InstallTask;
using ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.PostProcess;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing Optifine, a Minecraft optimization mod.
    /// </summary>
    public class OptifineInstaller: SequenceProgressTask
    {
        readonly string MCVersion = "";

        readonly Folder Owner;

        readonly string Name;

        string installerpath { get => $"{Files.CacheDir}/Installer/optifine.jar"; }

        //The Java Program I made myself. It is just used to handle the optifine install task
        string programpath { get => $"{Files.CacheDir}/Installer/optifineinstaller.jar"; }

        string url = "";

        string mcjarpath = "";

        readonly JObject OptifineVersion;

        FuncProgressTask<int> InitProgressTask;

        AsyncProgressTask DownloadTask;

        CLITask CommandTask;

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
            MCVersion = mcversion;
            OptifineVersion = optifineversion;
            Init(optifineversion);
        }


        private void Init(JObject optifineversion)
        {
            string file = Localize.PathLocalize($"{Files.CacheDir}/Installer/optifine.jar");
            InitProgressTask = new();
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
            if (Owner.FindGame(MCVersion) == null)
            {
                DownloadTask.Add(new MinimalOriginalInstallTask(Owner, MCVersion, MCVersion, true));
            }
            DownloadTask.Add(download);
            Add(DownloadTask);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            string dir =
                $"{MCVersion}-{OptifineVersion["version"].ToString().Replace(" ", "_")}";

            CommandTask = new(java.JavaEXE, "-cp " +
                $"\"{installerpath}{Localize.LocalParser}{programpath}\" Program " +
                $"\"{Owner.Path}\"");
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
