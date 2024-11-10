using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceDownloader.Download.ModLoaders
{
    /// <summary>
    /// Quilt Mod Loader
    /// </summary>
    public class QuiltServer: ModLoaderServer
    {
        /// <summary>
        /// Initor
        /// </summary>
        public QuiltServer() : base("quilt") 
        {
            Mirrors.Add(new OfficialQuiltMirror());
            UpdateIndex();
        }

        /// <summary>
        /// Official Quilt Server
        /// </summary>
        public class OfficialQuiltMirror : ModLoaderMirror
        {
            HttpClient client = new();

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<bool> CheckBuild(string mcversion)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/game");
                var content = await response.Content.ReadAsStringAsync();
                var array = JArray.Parse(content);

                foreach (var item in array) 
                {
                    if (mcversion == item["version"].ToString()) return true;
                }
                return false;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<JArray> GetBuild(string mcversion)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/loader");
                var content = await response.Content.ReadAsStringAsync();
                return JArray.Parse(content);
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="modloaderinf"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<string> GetURL(JObject modloaderinf)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/loader");
                var content = await response.Content.ReadAsStringAsync();
                return JArray.Parse(content)[0]["url"].ToString();
            }
        }
    }
}
