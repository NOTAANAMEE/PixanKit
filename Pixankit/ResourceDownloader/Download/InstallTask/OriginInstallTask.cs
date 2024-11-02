using PixanKit.ResourceDownloader.Download;
using PixanKit.LaunchCore.GameModule;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Tasks;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// 
    /// </summary>
    public class OrdinaryInstallTask:MultiSequenceTask
    {
        /// <summary>
        /// The Return Game
        /// </summary>
        public GameBase? Game
        {
            get => _game;
        }

        GameBase? _game;

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="folder">The Owner Of The Game</param>
        /// <param name="name">The Name Of The Game</param>
        /// <param name="version">The Version Of Minecraft</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public OrdinaryInstallTask(Folder folder, string name, string version) 
        {
            JObject? jData = (JObject?)ServerList.MinecraftVersionServer.GetVersions()
                [version] ?? throw new ArgumentException($"Does not exist {version}");
            string path = folder.VersionDir + '/' + name;
            if (Directory.Exists(path)) throw new IOException($"Already Exists {path}");
            MultiAsyncTask asyncTask = new();
            LibraryCompletionTask lct;
            MultiThreadDownload dt;
            AssetCompletionTask act;
            Directory.CreateDirectory(path);

            Add(new MultiThreadDownload("", path + $"/{name}.json"));
           
            asyncTask.Add(dt = new MultiThreadDownload("", path + $"/{name}.jar"));
            asyncTask.Add(lct = new LibraryCompletionTask());
            asyncTask.Add(act = new AssetCompletionTask(folder));
            Add(asyncTask);

            processes[0].OnFinish += () =>
            {
                Console.WriteLine("Task 0 Finished");

                FileStream fs;
                try
                {
                    fs = new(path + $"/{name}.json", FileMode.Open);
                }
                catch(Exception ex) 
                {
                    Console.WriteLine($"Error Happened {ex.Message}");
                    return;
                }
                StreamReader sr = new(fs);
                JObject mcjData = JObject.Parse(sr.ReadToEnd());
                _game = new OrdinaryGame(path, mcjData);
                fs.Close();
                if(_game == null) throw new Exception();
                dt = new MultiThreadDownload(mcjData["downloads"]["client"]["url"].ToString(), 
                    path + $"/{name}.jar");
                lct.SetMinecraft(_game);
                act.SetJData(mcjData);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override async Task Running()
        {
            Logger.Info("PixanKit.ResourceDownloader", "Start Installing Minecraft");
            processes[0].Start();
            await processes[0].MainTask;
            if (Status != ProcessStatus.Running) return;
            Logger.Info("PixanKit.ResourceDownloader", "Process 0 finished. Start running process 1 2 3");
            processes[1].Start();
            processes[2].Start();
            processes[3].Start();
            _ = base.Running();
        }
    }
}
