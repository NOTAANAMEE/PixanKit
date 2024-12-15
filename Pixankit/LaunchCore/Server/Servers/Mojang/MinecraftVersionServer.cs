using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server.Servers.Mojang
{
    /// <summary>
    /// Mirror Servers For Minecraft Version Resources
    /// </summary>
    public class MinecraftVersionServer: ResourceServer
    {
        private Cache? cache;

        private record Cache(JArray Versions, DateTime UpdateTime);

        /// <summary>
        /// Init A MinecraftVersionServer
        /// </summary>
        public MinecraftVersionServer() {
            Mirrors = new List<MirrorServer>() 
            { new("", "https://piston-meta.mojang.com") };
            Current = Mirrors[0]; 
        }

        /// <summary>
        /// Get The Versions from the Mojang server.
        /// After called, the cache will be saved for 1 day
        /// </summary>
        /// <returns>The JArray that contains every version</returns>
        public async Task<JArray> GetVersionsAsync(CancellationToken token)
        {
            if (cache == null || DateTime.Now - cache.UpdateTime > TimeSpan.FromDays(1))
            {
                //UpdateIndex();
                await GetArrayFromNetwork(token);
            }
            return cache.Versions;
        }

        /// <summary>
        /// Get The Versions from the Mojang server.
        /// After called, the cache will be saved for 1 day
        /// </summary>
        /// <returns>The JArray that contains every version</returns>
        public JArray GetVersions()
        {
            var task =  GetVersions(new CancellationToken());
            task.Wait();
            return task.Result;
        }

        public async Task<JArray> GetVersions(CancellationToken token)
        {
            if (cache == null || DateTime.Now - cache.UpdateTime > TimeSpan.FromDays(1))
            {
                await Update(token);
            }
            return cache.Versions;
        }

        /// <summary>
        /// Get the latest release from the JArray that contains all the versions
        /// </summary>
        /// <param name="jArray">The return from GetVersions</param>
        /// <returns>JObject from the JArray returned by GetVersions</returns>
        public JObject? GetLatestRelease(JArray jArray) 
        {
            foreach (JToken token in jArray)
            {
                if (token["type"].ToString() == "release") 
                    return token as JObject;
            }
            return null;
        }

        /// <summary>
        /// Get the latest Snapshot from the JArray that contains all the versions
        /// </summary>
        /// <param name="jArray">The return from GetVersions</param>
        /// <returns>JObject from the JArray returned by GetVersions</returns>
        public JObject? GetLatestSnapshot(JArray jArray) 
        {
            foreach (JToken token in jArray)
            {
                if (token["type"].ToString() != "release")
                    return token as JObject;
            }
            return null;
        }

        /// <summary>
        /// Get the URL of Json for Specific Minecraft Version
        /// </summary>
        /// <param name="jObject">The JObject from the Array returned by GetVersions
        /// </param>
        /// <returns>The URL for Json</returns>
        public string GetJsonUrl(JObject jObject) 
        {
            return Replace(jObject["downloads"]["client"]["url"].ToString()); 
        }

        /// <summary>
        /// Get the URL of Minecraft Jar File
        /// </summary>
        /// <param name="jObject">The Json Object parsed from the Json file for 
        /// the specific Minecraft Version</param>
        /// <returns>The URL</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetMinecraftJarUrl(JObject jObject)
        {
            return Replace(jObject["downloads"]["client"]["url"].ToString());
        }

        /// <summary>
        /// Get The URL Of Json For Assets
        /// </summary>
        /// <param name="jObject">Official Json For Specific Version</param>
        /// <returns>The URL</returns>
        public string GetAssetsJsonUrl(JObject jObject)
        {
            return Replace(jObject["assetIndex"]["url"].ToString());
        }

        /// <summary>
        /// Update The Cache
        /// </summary>
        public async Task Update(CancellationToken token)
        {
            //UpdateIndex();
            GetArrayFromNetwork(token);
        }

        private async Task GetArrayFromNetwork(CancellationToken token)
        {
            HttpClient client = new();
            var ret = await client.GetAsync(
                Replace("https://piston-meta.mojang.com/mc/game/version_manifest.json"), token);
            string tmp = await ret.Content.ReadAsStringAsync();
            JObject jObj = JObject.Parse(tmp);
            cache = new Cache(jObj["versions"] as JArray, DateTime.Now);
        }
    }
}
