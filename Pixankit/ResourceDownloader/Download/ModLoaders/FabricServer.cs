using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.ModLoaders
{
    /// <summary>
    /// Fabric Server For Installation
    /// </summary>
    public class FabricServer : ModLoaderServer
    {
        /// <summary>
        /// Initor
        /// </summary>
        public FabricServer() : base("fabric")
        {
            Mirrors.Add(new OfficialFabricMirror());
            UpdateIndex();
        }

        /// <summary>
        /// The Official Fabric Server
        /// </summary>
        public class OfficialFabricMirror : ModLoaderMirror
        {
            HttpClient client = new();

            /// <summary>
            /// Initor
            /// </summary>
            public OfficialFabricMirror()
            {
                BaseURL = "";
                OriginURL = "";
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<bool> CheckBuild(string mcversion)
            {
                await Task.Run(() => { Logger.Info("PixanKit.ResourceDownloader", "Check MC Version"); });
                return true;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<JArray> GetBuild(string mcversion)
            {
                var response = await client.GetAsync("https://meta.fabricmc.net/v2/versions");
                var content = await response.Content.ReadAsStringAsync();
                JObject jobj = JObject.Parse(content);
                return jobj["loader"] as JArray ?? new JArray();
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="loaderInf"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<string> GetURL(JObject loaderInf)
            {
                var response = await client.GetAsync("https://meta.fabricmc.net/v2/versions");
                var content = await response.Content.ReadAsStringAsync();
                JObject jobj = JObject.Parse(content);
                string url = jobj["installer"][0]["url"].ToString();
                return Replace(url);
            }
        }

    }

}
