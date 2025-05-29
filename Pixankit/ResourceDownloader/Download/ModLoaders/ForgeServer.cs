using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;

namespace PixanKit.ResourceDownloader.Download.ModLoaders;

/// <summary>
/// Represents a Forge mod loader server.
/// </summary>
public class ForgeServer : ModLoaderServer
{
    /// <summary>
    /// Initor. Do not touch it
    /// </summary>
    [ModuleInitializer]
    public static void Init()
    {
        _ = new ForgeServer();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgeServer"/> class.
    /// </summary>
    public ForgeServer() : base("forge")
    {
        Mirrors.Add(new OfficialForgeMirror());
        UpdateIndex();
    }

    /// <summary>
    /// Represents the official Forge mirror server.
    /// </summary>
    public class OfficialForgeMirror : ModLoaderMirror
    {
        private readonly HttpClient _client = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficialForgeMirror"/> class.
        /// </summary>
        public OfficialForgeMirror()
        {
            BaseUrl = "https://files.minecraftforge.net";
            OriginalUrl = "";
        }

        /// <inheritdoc/>
        /// <param name="mcversion">The Minecraft version to check builds for.</param>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the builds exist.</returns>
        public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
        {
            var response = await _client.GetAsync(
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

        /// <inheritdoc/>
        /// <param name="mcversion">The Minecraft version to retrieve builds for.</param>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a JSON array of mod loader versions.
        /// </returns>
        public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
        {
            var response = await _client.GetAsync(
                $"https://files.minecraftforge.net/net/minecraftforge/forge/index_{mcversion}.html", token);
            if (token.IsCancellationRequested) return [];
            var content = await response.Content.ReadAsStreamAsync(token);
            if (token.IsCancellationRequested) return [];
            HtmlDocument document = new();
            document.Load(content);
            var nodes = document.DocumentNode.SelectNodes(
                "/html/body/main/div[2]/div[2]/div[2]/table/tbody/tr");
            JArray array = [];
            foreach (var node in nodes)
            {
                if (token.IsCancellationRequested) return [];
                array.Add(Parse(node));
            }
            return array;
        }

        private static JObject Parse(HtmlNode node)
        {
            var name = node.SelectSingleNode("td[1]").InnerText;
            var url = node.SelectSingleNode("td[3]/ul/li[2]/a[2]").Attributes["href"].Value;
            var release = node.SelectSingleNode("td[2]").Attributes["title"].Value;
            var source = "official";
            return new()
            {
                { "version", name.Trim() },
                { "url", url },
                { "source", source },
                { "release", release },
            };
        }

        /// <inheritdoc/>
        /// <param name="modloaderinf">A JSON object containing mod loader information.</param>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the URL of the mod loader installer.
        /// </returns>
        public override Task<string> GetUrl(JObject modloaderinf, CancellationToken token)
            => Task.FromResult(Replace(modloaderinf["url"]?.ToString() ?? ""));

    }
}