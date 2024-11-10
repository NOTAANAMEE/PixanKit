using HtmlAgilityPack;
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
    /// Optifine Server
    /// </summary>
    public class OptifineServer: ModLoaderServer
    {
        /// <summary>
        /// Initor
        /// </summary>
        public OptifineServer() : base("optifine")
        {

        }

        /// <summary>
        /// Official Optifine Mirror Server
        /// </summary>
        public class OfficialOptifineServer : ModLoaderMirror
        {
            HttpClient client = new();

            private async Task<HtmlNodeCollection> GetNodes()
            {
                var response = await client.GetAsync("https://optifine.net/downloads");
                var content = await response.Content.ReadAsStringAsync();
                var versions = new List<string>();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(content);
                return document.DocumentNode.SelectNodes(
                    "/html/body/table/tbody/tr[2]/td/span/h2 | " +
                    "/html/body/table/tbody/tr[2]/td/span/div[position() != last()] | " +
                    "/html/body/table/tbody/tr[2]/td/span/div[last()]/h2 | " +
                    "/html/body/table/tbody/tr[2]/td/span/div[last()]/div");
            }

            private JArray Parse(HtmlNode node)
            {
                JArray ret = new();
                var nodes = node.SelectNodes("/div/table/tbody/tr");
                foreach (var opt in nodes)
                {
                    ret.Add(new JObject()
                    {
                        { "version", opt.SelectSingleNode("/tr/td[1]").InnerText },
                        { "url",  opt.SelectSingleNode("/tr/td[3]/a").Attributes["href"].Value },
                        { "forge", opt.SelectSingleNode("/tr/td[5]").InnerText },
                        { "release", opt.SelectSingleNode("/tr/td[6]").InnerText }
                    });
                }
                return ret;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="mcversion"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<bool> CheckBuild(string mcversion)
            {
                HtmlNodeCollection nodes = await GetNodes();
                for (var i = 0; i < nodes.Count; i += 2) 
                {
                    if (nodes[i].InnerText == mcversion) return true;
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
                HtmlNodeCollection nodes = await GetNodes();
                HtmlNode? node = null;
                for (var i = 0; i < nodes.Count; i += 2)
                {
                    if (nodes[i].InnerText == mcversion)
                    {
                        node = nodes[i + 1];
                        break;
                    }
                }
                if (node == null) return new JArray();
                return Parse(node);
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="modloaderinf"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override async Task<string> GetURL(JObject modloaderinf)
            {
                var response = await client.GetAsync(modloaderinf["url"].ToString());
                var content = await response.Content.ReadAsStringAsync();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(content);
                return "https://optifine.net/" + document.DocumentNode.SelectSingleNode(
                    "/html/body/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/span/a"
                    ).Attributes["href"].Value;
            }
        }
    }
}
