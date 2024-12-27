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
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using HtmlAgilityPack;
using System.Runtime.CompilerServices;

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

        readonly string name;

        readonly string version;

        readonly string path;

        FuncProgressTask<int> InitTask = new();
        
        FileDownloadTask? jsondownload;
        
        readonly AsyncProgressTask asyncTask = new();
        
        LibraryCompletionTask? libraryTask;
        
        FileDownloadTask? jarTask;
        
        AssetsCompletionTask? assetsTask;


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
            this.name = name;
            this.version = version;
            path = folder.VersionDir + '/' + name;
            InitTask.Function += GetVersion;
            Init();
        }

        private void Init()
        {
            if (Directory.Exists(path)) throw new IOException($"Already Exists {path}");

            Directory.CreateDirectory(path);
            Add(InitTask);
            Add(jsondownload = new FileDownloadTask("", path + $"/{name}.json"));

            asyncTask.Add(jarTask = new FileDownloadTask("", path + $"/{name}.jar"));
            asyncTask.Add(libraryTask = new LibraryCompletionTask());
            asyncTask.Add(assetsTask = new AssetsCompletionTask());
            Add(asyncTask);

            jsondownload.OnFinish += Task1Finish;
        }

        private void Task1Finish(ProgressTask task)
        {
            Console.WriteLine("Task 0 Finished");

            JObject mcjData = JObject.Parse(
                File.ReadAllText(Localize.PathLocalize($"{path}/{name}.json")));
            _game = new OriginalGame(path, mcjData);
            if (_game == null) throw new Exception();
            jarTask?.SetURL(mcjData["downloads"]?["client"]?["url"]?.ToString() ?? "");
            libraryTask?.Set(_game);
            assetsTask?.Set(mcjData, _game);
        }

        private async Task<int> GetVersion(Action<double> report, CancellationToken token)
        {
            var jarray = await ServerList.MinecraftVersionServer.GetVersionsAsync(token);
            if (token.IsCancellationRequested) return 1;
            foreach (var item in jarray)
            {
                if (item["id"]?.ToString() == version)
                {
                    jsondownload?.SetURL(item["url"]?.ToString() ?? throw new Exception());
                    report?.Invoke(1);
                    return 0;
                }
            }
            throw new Exception();
        }
    }
}
