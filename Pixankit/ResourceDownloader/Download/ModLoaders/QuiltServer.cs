using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;

namespace PixanKit.ResourceDownloader.Download.ModLoaders;

/// <summary>
/// Represents a Quilt mod loader server.
/// </summary>
public class QuiltServer : ModLoaderServer
{
    /// <summary>
    /// Initor. Don't touch it
    /// </summary>
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
        readonly HttpClient _client = new();

        /// <summary>
        /// 
        /// </summary>
        public OfficialQuiltMirror()
        {
            BaseUrl = "https://meta.quiltmc.org";
        }

        /// <inheritdoc/>
        public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
        {
            var response = await _client.GetAsync("https://meta.quiltmc.org/v3/versions/game", token);
            var content = await response.Content.ReadAsStringAsync(token);
            var array = JArray.Parse(content);

            foreach (var item in array)
            {
                if (mcversion == item["version"]?.ToString()) return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
        {
            var response = await _client.GetAsync("https://meta.quiltmc.org/v3/versions/loader", token);
            if (token.IsCancellationRequested) return [];
            var content = await response.Content.ReadAsStringAsync(token);
            if (token.IsCancellationRequested) return [];
            return JArray.Parse(content);
        }

        /// <inheritdoc/>
        public override async Task<string> GetUrl(JObject modloaderinf, CancellationToken token)
        {
            var url = "https://meta.quiltmc.org/v3/versions/installer";
            var response = await _client.GetAsync(url, token);
            var content = await response.Content.ReadAsStringAsync(token);
            var array = JArray.Parse(content);
            return array[0]["url"]?.ToString() ?? "";
        }
    }
}