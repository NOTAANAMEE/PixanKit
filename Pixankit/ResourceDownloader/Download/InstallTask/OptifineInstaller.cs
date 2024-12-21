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

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing Optifine, a Minecraft optimization mod.
    /// </summary>
    public class OptifineInstaller: SequenceProgressTask
    {
        string MCVersion = "";

        Folder Owner;

        string Name;

        string version;

        string installerpath { get => $"{Files.CacheDir}/Installer/optifine.jar"; }

        string url = "";

        JObject OptifineVersion;

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
            version = optifineversion["version"].ToString();
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
            FileDownloadTask download = new("", installerpath);
            InitProgressTask.OnFinish += (a) =>
            {
                download.SetURL(url);
            };
            download.OnFinish += (a) => { UnpressWrapperFile(); };
            if (Owner.FindGame(MCVersion) == null)
                DownloadTask.Add(new MinimalOriginalInstallTask(Owner, MCVersion, MCVersion, true));
            DownloadTask.Add(download);
            Add(DownloadTask);
        }

        private void AddCommandTask()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            var id = version[(version.LastIndexOf('-') + 1)..];
            var mcpath = Localize.PathLocalize(Owner.VersionDir + $"/{Name}");
            var librarypath = Localize.PathLocalize(
                $"{Owner.LibraryDir}/optifine/Optifine/{id}/{version}.jar");
            Localize.CheckDir(mcpath);
            Localize.CheckDir(librarypath);

            CommandTask = new(java.JavaEXE,
                "-cp " +
               $"\"{Localize.PathLocalize(installerpath)}\" " +
                "optifine.Patcher " +
               $"\"{mcpath}\" " +
               $"\"{Localize.PathLocalize(librarypath)}\"");
            CommandTask.OnFinish += (a) =>
            {
                ModLoaderServer.Move(Owner, "", Name);
            };
            Add(CommandTask);
        }

        private void UnpressWrapperFile()
        {
            string extractpath;
            string config = Localize.PathLocalize(Files.CacheDir + "/optifine.txt");
            FileStream fs = new(Localize.PathLocalize(installerpath), FileMode.Open);
            ZipArchive archive = new(fs);
            var entry = archive.GetEntry("launcherwrapper-of.txt");
            entry.ExtractToFile(config);
            string content = File.ReadAllText(config).Trim();

            extractpath = Localize.PathLocalize(
                    Owner.LibraryDir +
                    $"/optifine/launcherwrapper-of/{content}/" +
                    $"launcherwrapper-of-{content}.jar");
            Localize.CheckDir(extractpath);

            var extractfile = archive.GetEntry($"launcherwrapper-of-{content}.jar");
            if (extractfile != null)
                extractfile.ExtractToFile(extractpath);
            File.Delete(config);

            archive.Dispose();
            fs.Close();
        }

        private async Task<int> GetURL(Action<double> report, CancellationToken token)
        {
            url = await ServerList.ModLoaderServers["optifine"]
                    .GetURL(OptifineVersion, token);
            return 0;
        }
    }
}
