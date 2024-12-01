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
                OriginalURL = "";
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Task<bool> CheckBuild(string mcversion, CancellationToken token)
                => Task.FromResult(true);


            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
            {
                var response = await client.GetAsync("https://meta.fabricmc.net/v2/versions", token);
                if (token.IsCancellationRequested) return new();
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return new();
                JObject jobj = JObject.Parse(content);
                return jobj["loader"] as JArray ?? new JArray();
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="loaderInf"><inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<string> GetURL(JObject loaderInf, CancellationToken token)
            {
                var response = await client.GetAsync("https://meta.fabricmc.net/v2/versions", token);
                if (token.IsCancellationRequested) return "";
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return "";
                JObject jobj = JObject.Parse(content);
                string url = jobj["installer"][0]["url"].ToString();
                return Replace(url);
            }
        }

    }

}
