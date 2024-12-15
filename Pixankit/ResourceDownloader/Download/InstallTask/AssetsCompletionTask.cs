using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for completing Minecraft game assets, including downloading the asset index
    /// and the associated asset files.
    /// </summary>
    public class AssetsCompletionTask:SequenceProgressTask
    {
        GameBase _game;

        string indexpath = "";

        private MultiFileDownloadTask task2;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetsCompletionTask"/> class.
        /// </summary>
        /// <param name="jdata">The JSON data containing the asset index information.</param>
        /// <param name="game">The game context to manage assets.</param>
        public AssetsCompletionTask(JObject jdata, GameBase game)
        {
            _game = game;
            Init(jdata);
        }


        internal AssetsCompletionTask() { }

        internal void Set(JObject jdata, GameBase game)
        {
            _game = game;
            Init(jdata);
        }

        private void Init(JObject jdata)
        {
            FileDownloadTask task;
            
            string url = jdata["assetIndex"]["url"].ToString();
            string index = jdata["assetIndex"]["id"].ToString();

            indexpath = $"{_game.AssetsDir}/indexes/{index}.json";

            if (File.Exists(Localize.PathLocalize(indexpath)))
            {
                Add(task = new FileDownloadTask(url, indexpath));
                task.OnFinish += TaskFinish;
            }
            else TaskFinish(null);
            Add(task2 = new MultiFileDownloadTask());
        }
        
        private void TaskFinish(ProgressTask t)
        {
            List<string> urls = [], paths = [];

            JObject jobj = JObject.Parse(File.ReadAllText(
                    Localize.PathLocalize(indexpath)
                    ));

            var count = 0;
            foreach (var asset in jobj["objects"])
            {
                try
                {
                    string hash = asset.First["hash"].ToString();
                    string rpath = $"{hash[0..2]}/{hash}";
                    string path = $"{_game.AssetsDir}/objects/{rpath}";
                    Console.WriteLine(++count);
                    if (File.Exists(Localize.PathLocalize(path))) continue;
                    urls.Add(ServerList.MinecraftAssetsServer.GetAssetsUrl(hash));
                    paths.Add(path);
                }
                catch(Exception ex) { Console.WriteLine(ex.Message); }

            }
            task2.Set([.. urls], [.. paths]);
        }

    }
}
