using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;

namespace PixanKit.ResourceDownloader.Download.ModLoaders;

/// <summary>
/// Optifine Server
/// </summary>
public class OptifineServer : ModLoaderServer
{
    /// <summary>
    /// Initor. Dont touch it
    /// </summary>
    [ModuleInitializer]
    public static void Init()
    {
        _ = new OptifineServer();
    }


    /// <summary>
    /// Initor
    /// </summary>
    public OptifineServer() : base("optifine")
    {
        Mirrors.Add(new OfficialOptifineServer());
        UpdateIndex();
    }

    /// <summary>
    /// Official Optifine Mirror Server
    /// </summary>
    public class OfficialOptifineServer : ModLoaderMirror
    {
        private readonly HttpClient _client = new();

        /// <summary>
        /// 
        /// </summary>
        public OfficialOptifineServer()
        {
            BaseUrl = "https://optifine.net";
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");//UA
        }

        private async Task<HtmlNodeCollection?> GetNodes(CancellationToken token)
        {
            var response = await _client.GetAsync("https://optifine.net/downloads", token);
            if (token.IsCancellationRequested) return null;
            var content = await response.Content.ReadAsStringAsync(token);
            if (token.IsCancellationRequested) return null;
            //var versions = new List<string>();
            HtmlDocument document = new();
            document.LoadHtml(content);
            await File.WriteAllTextAsync("a.html", content, token);
            if (token.IsCancellationRequested) return null;
            var xpath = "/html/body/table/tr[2]/td/span//tr | " +
                        "/html/body/table/tr[2]/td/span//h2";//WTF no tbody
            return document.DocumentNode.SelectNodes(xpath);
        }

        private static JObject Parse(HtmlNode node)
        {
            return new JObject()
            {
                { "version", node.SelectSingleNode("td[1]").InnerText },
                { "url", node.SelectSingleNode("td[3]/a").Attributes["href"].Value },
                { "forge", node.SelectSingleNode("td[5]").InnerText },
                { "release", node.SelectSingleNode("td[6]").InnerText }
            };
        }

        private static int GetStartIndex(HtmlNodeCollection collection, string gameVersion)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                var node = collection[i];
                if (node.Name == "h2" && node.InnerText == $"Minecraft {gameVersion}")
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="mcversion"><inheritdoc/></param>
        /// <param name="token"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
        {
            var nodes = await GetNodes(token);
            return nodes != null && GetStartIndex(nodes, mcversion) != -1;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="token"><inheritdoc/></param>
        /// <param name="mcversion"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
        {
            var nodes = await GetNodes(token);
            JArray array = [];
            if (nodes == null) return array;
            if (token.IsCancellationRequested) return array;
            var index = GetStartIndex(nodes, mcversion);
            if (index == -1 || token.IsCancellationRequested) return array;

            for (var i = index + 1; i < nodes.Count; i++)
            {
                if (nodes[i].Name != "tr") break;
                array.Add(Parse(nodes[i]));
            }
            return array;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="modloaderinf"><inheritdoc/></param>
        /// <param name="token"><inheritdoc/></param>
        /// <returns></returns>
        public override async Task<string> GetUrl(JObject modloaderinf, CancellationToken token)
        {
            var response = await _client.GetAsync(modloaderinf["url"]?.ToString(), token);
            if (token.IsCancellationRequested) return "";
            var content = await response.Content.ReadAsStringAsync(token);
            if (token.IsCancellationRequested) return "";

            HtmlDocument document = new();
            document.LoadHtml(content);
            if (token.IsCancellationRequested) return "";
            return "https://optifine.net/" + document.DocumentNode.SelectSingleNode(
                "/html/body/table/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/span/a"
            ).Attributes["href"].Value;
        }
    }
}