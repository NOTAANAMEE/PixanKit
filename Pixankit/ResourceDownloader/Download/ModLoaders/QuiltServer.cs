using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.ModLoaders
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
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/game", token);
                if (token.IsCancellationRequested) return false;
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return false;
                var array = JArray.Parse(content);

                foreach (var item in array) 
                {
                    if (token.IsCancellationRequested) return false;
                    if (mcversion == item["version"].ToString()) return true;
                }
                return false;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/loader", token);
                if (token.IsCancellationRequested) return new JArray();
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return new JArray();
                return JArray.Parse(content);
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="modloaderinf"><inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<string> GetURL(JObject modloaderinf, CancellationToken token)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/loader", token);
                if (token.IsCancellationRequested) return "";
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return "";
                return JArray.Parse(content)[0]["url"].ToString();
            }
        }
    }
}
