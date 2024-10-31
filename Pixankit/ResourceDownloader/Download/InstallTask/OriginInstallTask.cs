using PixanKit.LaunchCore.Downloads;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Tasks.MultiTasks;
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

namespace PixanKit.LaunchCore.Download.InstallTask
{
    /// <summary>
    /// 
    /// </summary>
    public class OrdinaryInstallTask:MultiSequenceTask
    {
        /// <summary>
        /// 
        /// </summary>
        public GameBase? Game
        {
            get => _game;
        }

        GameBase? _game;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public OrdinaryInstallTask(Folder folder, string name, string version) 
        {
            JObject? jData = (JObject?)ServerList.MinecraftVersionServer.GetVersions()
                [version] ?? throw new ArgumentException($"Does not exist {version}");
            string path = folder.VersionDir + '/' + name;
            MultiAsyncTask asyncTask = new();
            LibraryCompletionTask lct;
            DownloadTask dt;
            AssetCompletionTask act;
            Directory.CreateDirectory(path);

            Add(new DownloadTask(jData["url"].ToString(), path + $"/{name}.json"));
           
            asyncTask.Add(dt = new DownloadTask(path + $"/{name}.jar"));
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
                JObject jData = JObject.Parse(sr.ReadToEnd());
                _game = new OrdinaryGame(path, jData);
                fs.Close();
                if(_game == null) throw new Exception();
                dt.SetURL(ServerList.MinecraftVersionServer.GetMinecraftJarUrl(jData));
                lct.SetGame(_game);
                act.SetJData(jData);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override async Task Running()
        {
            Logger.Info("Start Installing Minecraft");
            processes[0].Start();
            await processes[0].MainTask;
            if (Status != PixanKit.LaunchCore.Tasks.ProcessStatus.Running) return;
            Logger.Info("Process 0 finished. Start running process 1 2 3");
            processes[1].Start();
            processes[2].Start();
            processes[3].Start();
            _ = base.Running();
        }
    }
}
