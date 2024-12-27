using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResourceDownloader.Download.InstallTask
{
    public class MinimalOriginalInstallTask : SequenceProgressTask
    {
        readonly Folder Owner;

        readonly string name;

        readonly string version;

        readonly string path;

        readonly FuncProgressTask<int> FuncProgressTask = new();

        FileDownloadTask jsondownload;

        FileDownloadTask jardownload;

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="folder">The Owner Of The Game</param>
        /// <param name="name">The Name Of The Game</param>
        /// <param name="version">The Version Of Minecraft</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public MinimalOriginalInstallTask(Folder folder, string name, string version)
        {
            this.Owner = folder;
            this.name = name;
            this.version = version;
            path = folder.VersionDir + '/' + name;
            FuncProgressTask.Function += GetVersion;
            Init();
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="folder">The Owner Of The Game</param>
        /// <param name="name">The Name Of The Game</param>
        /// <param name="version">The Version Of Minecraft</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public MinimalOriginalInstallTask(Folder folder, string name, string version, bool withjar):this(folder, name, version) 
        {
            if (!withjar) return;
            if (File.Exists(path + $"/{name}.jar")) return;
            Add(jardownload = new("", path + $"/{name}.jar"));
            jsondownload.OnFinish += Task1Finish;
        }

        private void Init()
        {
            if (File.Exists(path + $"/{name}.json"))
            {
                Task1Finish(new SequenceProgressTask());
                return;
            }

            Directory.CreateDirectory(path);
            Add(FuncProgressTask);
            Add(jsondownload = new FileDownloadTask("", path + $"/{name}.json"));
        }


        private async Task<int> GetVersion(Action<double> report, CancellationToken token)
        {
            var jarray = await ServerList.MinecraftVersionServer.GetVersionsAsync(token);
            if (token.IsCancellationRequested) return 1;
            foreach (var item in jarray)
            {
                if (item["id"].ToString() == version)
                {
                    jsondownload.SetURL(item["url"].ToString());
                    report?.Invoke(1);
                    return 0;
                }
            }
            throw new Exception();
        }

        private void Task1Finish(ProgressTask task)
        {
            Console.WriteLine("Task 0 Finished");
            JObject mcjData = JObject.Parse(
                File.ReadAllText(Localize.PathLocalize($"{path}/{name}.json")));
            jardownload?.SetURL(mcjData["downloads"]["client"]["url"].ToString());
        }
    }
}
