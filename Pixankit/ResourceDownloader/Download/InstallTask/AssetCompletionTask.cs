using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using PixanKit.LaunchCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using PixanKit.ResourceDownloader.Download;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.SystemInf;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Complete Installing The Assets
    /// </summary>
    public class AssetCompletionTask:MultiFileDownload
    {
        Folder folder;

        JObject? JData;

        /// <summary>
        /// Assets Completion Task
        /// </summary>
        /// <param name="folder">The Folder Of Minecraft</param>
        /// <param name="jData">Minecraft Version JSON File</param>
        public AssetCompletionTask(Folder folder, JObject jData):this(folder) 
        {
            JData = jData;
            Init().Wait();
        }

        /// <summary>
        /// Ahead Initor
        /// </summary>
        /// <param name="folder">Folder Of Minecraft</param>
        public AssetCompletionTask(Folder folder): base()
        {
            this.folder = folder;
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
            foreach (var item in lists) 
            {
                if (File.Exists(Localize.PathLocalize(item.Path))) continue;
                Add(item.Url, folder.LibraryDir +  item.Path);
            }
        }

        private async Task<List<FileInf>> GetAssets() 
        {
            HttpClient client = new();
            var response = await client.GetAsync(JData["assetIndex"]["url"].ToString());
            JData = JObject.Parse(await response.Content.ReadAsStringAsync());
            List<FileInf> ret = new();
            foreach (var item in JData["objects"])
            {
                ret.Add(
                    new FileInf(
                        ServerList.MinecraftAssetsServer.GetAssetsUrl(item as JObject),
                        ServerList.MinecraftAssetsServer.GetFileLocation(item as JObject)
                        ));
            }
            return ret;
        }

        private record FileInf(string Url, string Path);
    }
}
