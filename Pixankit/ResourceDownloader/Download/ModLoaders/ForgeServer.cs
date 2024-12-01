using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
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
    /// Forge Install Server
    /// </summary>
    public class ForgeServer: ModLoaderServer
    {
        /// <summary>
        /// Initor
        /// </summary>
        public ForgeServer() : base("forge")
        {
            Mirrors.Add(new OfficialForgeMirror());
            UpdateIndex();
        }

        /// <summary>
        /// Official forge Mirror Server
        /// </summary>
        public class OfficialForgeMirror : ModLoaderMirror
        {
            HttpClient client = new();

            /// <summary>
            /// Initor
            /// </summary>
            public OfficialForgeMirror()
            {
                BaseURL = "https://files.minecraftforge.net";
            }

            /// <summary>
            ///  <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"> <inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns> <inheritdoc/></returns>
            public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
            {
                var response = await client.GetAsync(
                    "https://files.minecraftforge.net/net/minecraftforge/forge/", token);
                if (token.IsCancellationRequested) return false;
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return false;
                HtmlDocument document = new();
                document.LoadHtml(content);
                var nodes = document.DocumentNode.SelectNodes(
                    "/html/body/main/div[1]/aside/section/ul/div/div[1]/ul/li/ul/li/a");
                foreach (var node in nodes)
                {
                    if (token.IsCancellationRequested) return false;
                    if (node.InnerHtml == mcversion) return true;
                }
                return false;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"> <inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns> <inheritdoc/></returns>
            public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
            {
                var response = await client.GetAsync(
                    $"https://files.minecraftforge.net/net/minecraftforge/forge/index_{mcversion}.html", token);
                if (token.IsCancellationRequested) return new JArray();
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return new JArray();
                HtmlDocument document = new();
                document.Load(content);
                var nodes = document.DocumentNode.SelectNodes(
                    "/html/body/main/div[2]/div[2]/div[2]/table/tbody/tr");
                JArray array = new();
                foreach (var node in nodes)
                {
                    if (token.IsCancellationRequested) return new JArray();
                    array.Add(Parse(node));
                }
                return array;
            }

            private JObject Parse(HtmlNode node)
            {
                string name = node.SelectSingleNode("/td[1]").InnerText;
                string url = node.SelectSingleNode("/td[3]/ul/li[2]/a[2]").Attributes["href"].Value;
                string release = node.SelectSingleNode("/td[2]").Attributes["title"].Value;
                string source = "official";
                return new()
                {
                    { "version", name },
                    { "url", url },
                    { "source", source },
                    { "release", release },
                };
            }

            /// <summary>
            ///  <inheritdoc/>
            /// </summary>
            /// <param name="modloaderinf"><inheritdoc/></param>
            /// <param name="token"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Task<string> GetURL(JObject modloaderinf, CancellationToken token)
                => Task.FromResult(Replace(modloaderinf["url"].ToString()));

        }
    }

}
