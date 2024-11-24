using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.ModLoaders;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.JavaModule;
using System.IO.Compression;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Optifine Install Task
    /// </summary>
    public class OptifineInstaller: MultiSequenceTask
    {
        string MCVersion = "";

        Folder Owner;

        string Name;

        string version;

        string installerpath = "";

        /// <summary>
        /// Init an Optifine installer
        /// </summary>
        /// <param name="folder">The Target Minecraft Folder</param>
        /// <param name="name">The Actual Minecraft Name. The path will be folder\name\name.jar</param>
        /// <param name="optifineversion">The Optifine Version</param>
        /// <param name="mcversion">Minecraft Version</param>
        public OptifineInstaller(Folder folder, string name, string mcversion, JObject optifineversion) 
        {
            Name = name;
            Owner = folder;
            MCVersion = mcversion;
            Init_CheckExists(folder);
            version = optifineversion["version"].ToString();
        }

        /// <summary>
        /// This method will check whether version exists in this folder.
        /// If exists, skil. If not, add a Minecraft install task
        /// </summary>
        /// <param name="folder"></param>
        private void Init_CheckExists(Folder folder) 
        {
            if (folder.FindVersion(MCVersion, GameType.Ordinary) != null) return;
            Add(new OrdinaryInstallTask(folder, MCVersion, MCVersion));
        }

        private void InitDownload(JObject optifineversion)
        {
            string file = Localize.PathLocalize($"{Files.CacheDir}/Installer/optifine.jar");
            TrackFuncTask<string> funcTask = new();
            funcTask.Function += async (a, b) =>
            {
                var tmp = await (ServerList.ModLoaderServers["optifine"] as OptifineServer)
                    .GetURL(optifineversion, b);
                a.Sched = 10;
                return tmp;
            };
            MultiThreadDownload download = new(file);
            funcTask.OnFinish += () =>
            {
                download.SetURL(funcTask.Return);
            };
            download.OnFinish += UnpressWrapperFile;
        }

        private void ArgGenerator()
        {
            var java = JavaChooser.Newest(Launcher.Instance.JavaRuntimes);
            var id = version[(version.LastIndexOf('_') + 1)..];
            var mcpath = Localize.PathLocalize(Owner.VersionDir + $"/{Name}");
            var librarypath = Localize.PathLocalize(
                $"{Owner.LibraryDir}/optifine/Optifine/{id}/{installerpath}");
            Localize.CheckDir(mcpath);
            Localize.CheckDir(librarypath);

            CLIRunningTask task = new(java.JavaEXE,
                "-cp " +
               $"\"{Localize.PathLocalize(installerpath)}\" " +
                "optifine.Patcher " +
               $"\"{mcpath}\" " +
               $"\"{Localize.PathLocalize(librarypath)}\"");
            task.OnFinish += () =>
            {
                Owner.AddGame(new ModifiedGame(mcpath));
            };
            Add(task);
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
                    $"optifine/launcherwrapper-of/{content}/" +
                    $"launcherwrapper-of-{content}.jar");
            Localize.CheckDir(extractpath);

            var extractfile = archive.GetEntry($"launcherwrapper-of-{content}.jar");
            if (!File.Exists(extractpath))
                extractfile.ExtractToFile(extractpath);
            File.Delete(config);

            archive.Dispose();
            fs.Close();
        }
    }
}
