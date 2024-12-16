using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.ModLoaders
{
    /// <summary>
    /// Represents a Quilt mod loader server.
    /// </summary>
    public class QuiltServer: ModLoaderServer
    {
        [ModuleInitializer]
        public static void Init()
        {
            _ = new QuiltServer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuiltServer"/> class.
        /// </summary>
        public QuiltServer() : base("quilt") 
        {

            Mirrors.Add(new OfficialQuiltMirror());
            UpdateIndex();
        }

        /// <summary>
        /// Represents the official Quilt mirror server.
        /// </summary>
        public class OfficialQuiltMirror : ModLoaderMirror
        {
            HttpClient client = new();

            public OfficialQuiltMirror()
            {
                BaseURL = "https://meta.quiltmc.org";
            }

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
            {
                var response = await client.GetAsync("https://meta.quiltmc.org/v3/versions/loader", token);
                if (token.IsCancellationRequested) return new JArray();
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return new JArray();
                return JArray.Parse(content);
            }

            /// <inheritdoc/>
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
