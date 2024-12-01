using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using PixanKit.ResourceDownloader.SystemInf;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// 
    /// </summary>
    public class OriginalInstallTask : SequenceProgressTask
    {
        /// <summary>
        /// The Return Game
        /// </summary>
        public GameBase? Game
        {
            get => _game;
        }

        GameBase? _game;

        Folder folder;

        string name;

        string version;

        string path;

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="folder">The Owner Of The Game</param>
        /// <param name="name">The Name Of The Game</param>
        /// <param name="version">The Version Of Minecraft</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public OriginalInstallTask(Folder folder, string name, string version)
        {
            this.folder = folder;
            this.name = name;
            this.version = version;
            path = folder.VersionDir + '/' + name;
        }

        private void Init()
        {
            JObject? jData = (JObject?)ServerList.MinecraftVersionServer.GetVersions()
                [version] ?? throw new ArgumentException($"Does not exist {version}");
            
            if (Directory.Exists(path)) throw new IOException($"Already Exists {path}");
            AsyncProgressTask asyncTask = new();
            LibraryCompletionTask lct;
            FileDownloadTask dt;
            AssetsCompletionTask act;
            Directory.CreateDirectory(path);

            Add(new FileDownloadTask("", path + $"/{name}.json"));

            asyncTask.Add(dt = new FileDownloadTask("", path + $"/{name}.jar"));
            asyncTask.Add(lct = new LibraryCompletionTask());
            asyncTask.Add(act = new AssetsCompletionTask());
            Add(asyncTask);

            ProgressTasks[0].OnFinish += Task1Finish;
        }

        private void Task1Finish(ProgressTask task)
        {
            Console.WriteLine("Task 0 Finished");
            FileDownloadTask dt = ProgressTasks[1] as FileDownloadTask;
            LibraryCompletionTask lct = ProgressTasks[2] as LibraryCompletionTask;
            AssetsCompletionTask act = ProgressTasks[3] as AssetsCompletionTask;

            JObject mcjData = JObject.Parse(
                File.ReadAllText(Localize.PathLocalize($"{path}/{name}.json")));
            _game = new OriginalGame(path, mcjData);
            if (_game == null) throw new Exception();
            dt.SetURL(mcjData["downloads"]["client"]["url"].ToString());
            lct.Set(_game);
            act.Set(mcjData, _game);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override async Task Running()
        {
            Logger.Info("PixanKit.ResourceDownloader", "Start Installing Minecraft");
            ProgressTasks[0].Start();
            await ProgressTasks[0].MainTask;
            if (Status != ProgressStatus.Running) return;
            Logger.Info("PixanKit.ResourceDownloader", "Process 0 finished. Start running process 1 2 3");
            ProgressTasks[1].Start();
            ProgressTasks[2].Start();
            ProgressTasks[3].Start();
            _ = base.Running();
        }
    }
}
