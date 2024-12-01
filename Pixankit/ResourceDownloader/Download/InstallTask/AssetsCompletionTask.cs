using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    public class AssetsCompletionTask:SequenceProgressTask
    {
        GameBase _game;

        string indexpath = "";

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
            FileDownloadTask task;MultiFileDownloadTask task2;
            
            string url = jdata["assetIndex"]["url"].ToString();
            string index = jdata["assetIndex"]["id"].ToString();

            indexpath = $"{_game.AssetsDir}/indexes/{index}.json";

            Add(task = new FileDownloadTask(url, indexpath));
            Add(task2 = new MultiFileDownloadTask());
            task.OnFinish += (a) =>
            {
                List<string> urls = new(), paths = new();

                JObject jobj = JObject.Parse(File.ReadAllText(
                        Localize.PathLocalize(indexpath)
                        ));

                foreach (JObject asset in jobj["objects"])
                {
                    string hash = asset["hash"].ToString();
                    string path = $"{hash[0..2]}/{hash}";

                    urls.Add(ServerList.MinecraftAssetsServer.GetAssetsUrl(hash));
                    paths.Add($"{_game.AssetsDir}/objects/{path}");
                }
                task2.Set(urls.ToArray(), paths.ToArray());
            };
        }
    }
}
