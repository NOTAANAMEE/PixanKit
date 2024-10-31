using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Tasks.MultiTasks;
using PixanKit.LaunchCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Downloads;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Server;

namespace PixanKit.LaunchCore.Download.InstallTask
{
    /// <summary>
    /// Complete Installing The Assets
    /// </summary>
    public class AssetCompletionTask:MultiThreadDownloadTask
    {
        /// <summary>
        /// Maximum Thread Num
        /// </summary>
        public static short ThreadNum = 64;

        Folder folder;

        JObject? JData;

        /// <summary>
        /// Finished Files
        /// </summary>
        public int FinishedCount = 0;

        /// <summary>
        /// Total Assets
        /// </summary>
        public int Total;

        /// <summary>
        /// Assets Completion Task
        /// </summary>
        /// <param name="folder">The Folder Of Minecraft</param>
        /// <param name="jData">Minecraft Version JSON File</param>
        public AssetCompletionTask(Folder folder, JObject jData):base() 
        {
            this.folder = folder;
            JData = jData;
            for (int i = 0; i < ThreadNum; i++)
            {
                Add(new MultiDownload());
            }
            Init().Wait();
        }

        /// <summary>
        /// Ahead Initor
        /// </summary>
        /// <param name="folder">Folder Of Minecraft</param>
        public AssetCompletionTask(Folder folder): base()
        {
            this.folder = folder;
            for (int i = 0; i < ThreadNum; i++)
            {
                Add(new MultiDownload());
            }
        }

        /// <summary>
        /// Set The Minecraft JSON Data
        /// </summary>
        /// <param name="jData">Minecraft JSON Data</param>
        public void SetJData(JObject jData)
        {
            JData = jData;
            Init().Wait();
        }

        private async Task Init()
        {
            if (folder == null || JData == null) throw new NullReferenceException();
            var lists = await GetAssets();
            int ind = 0;
            foreach (var list in lists) 
            {
                if (File.Exists(Localize.PathLocalize(list.Item2))) continue;
                var tmp = processes[ind % ThreadNum] as MultiDownload;
                tmp.Add(new DownloadTask(list.Item1, list.Item2));
                ind++;
                tmp.OnFinish += () => {
                    FinishedCount++;
                };
            }
            Total = ind;
        }

        private async Task<List<(string, string)>> GetAssets() 
        {
            HttpClient client = new();
            var response = await client.GetAsync(JData["assetIndex"]["url"].ToString());
            JData = JObject.Parse(await response.Content.ReadAsStringAsync());
            List<(string, string)> ret = new();
            foreach (var item in JData["objects"])
            {
                ret.Add((ServerList.MinecraftAssetsServer.GetAssetsUrl(item as JObject),
                    ServerList.MinecraftAssetsServer.GetAssetsUrl(item as JObject)));
            }
            return ret;
        }
    }
}
