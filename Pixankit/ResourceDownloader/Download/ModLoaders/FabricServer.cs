using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Exceptions;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;

namespace PixanKit.ResourceDownloader.Download.ModLoaders;

/// <summary>
/// Represents a Fabric mod loader server for managing Fabric installations.
/// </summary>
/// <remarks>
/// This class handles the initialization and management of Fabric mod loader mirrors
/// and provides functionality to fetch build information and URLs for Fabric installations.
/// </remarks>
public class FabricServer : ModLoaderServer
{
    /// <summary>
    /// Initor. Do not touch it
    /// </summary>
    [ModuleInitializer]
    public static void Init()
    {
        _ = new FabricServer();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FabricServer"/> class.
    /// </summary>
    /// <remarks>
    /// By default, this class uses the official Fabric mirror and updates the index on initialization.
    /// </remarks>
    public FabricServer() : base("fabric")
    {
        Mirrors.Add(new OfficialFabricMirror());
        UpdateIndex();
    }

    /// <summary>
    /// Represents the official Fabric mirror used for fetching build and installer information.
    /// </summary>
    public class OfficialFabricMirror : ModLoaderMirror
    {
        private readonly HttpClient _client = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficialFabricMirror"/> class.
        /// </summary>
        /// <remarks>
        /// The base URL and Vanilla URL properties are initialized as empty by default.
        /// </remarks>
        public OfficialFabricMirror()
        {
            BaseUrl = "https://meta.fabricmc.net/";
            OriginalUrl = "";
        }

        /// <summary>
        /// Checks if a build is available for the specified Minecraft version.
        /// </summary>
        /// <param name="mcversion">The Minecraft version to check.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains <c>true</c> if a build is available; otherwise, <c>false</c>.
        /// </returns>
        public override Task<bool> CheckBuild(string mcversion, CancellationToken token)
        {
            if (mcversion.Contains('.')) return CheckReleaseBuild(mcversion);
            return CheckSnapBuild(mcversion);
        }


        private Task<bool> CheckReleaseBuild(string gameVersion)
        {
            var version = gameVersion.Split('.');
            return Task.FromResult(int.Parse(version[1]) >= 14);
        }

        private Task<bool> CheckSnapBuild(string gameVersion)
        {
            return Task.FromResult(string.Compare(gameVersion, "18w43b", StringComparison.Ordinal) >= 0);
        }

        /// <summary>
        /// Retrieves the build information for the specified Minecraft version.
        /// </summary>
        /// <param name="mcversion">The Minecraft version to fetch builds for.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="JArray"/> of build information.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if there is an error during the HTTP request.
        /// </exception>
        public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
        {
            var response = await _client.GetAsync("https://meta.fabricmc.net/v2/versions", token);
            if (token.IsCancellationRequested) return [];
            var content = await response.Content.ReadAsStringAsync(token);
            if (token.IsCancellationRequested) return [];
            var jObject = JObject.Parse(content);
            return jObject["loader"] as JArray ?? [];
        }

        /// <summary>
        /// Retrieves the download URL for the specified Fabric loader information.
        /// </summary>
        /// <param name="loaderInf">The JSON object containing loader information.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the download URL as a string.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if there is an error during the HTTP request.
        /// </exception>
        public override async Task<string> GetUrl(JObject loaderInf, CancellationToken token)
        {
            var response = await _client.GetAsync("https://meta.fabricmc.net/v2/versions", token);
            if (token.IsCancellationRequested) return "";
            var content = await response.Content.ReadAsStringAsync(token);
            if (token.IsCancellationRequested) return "";
            var jObject = JObject.Parse(content);
            var url = jObject["installer"]?[0]?["url"]?.ToString() ??
                      throw new JsonKeyException(jObject, "/installer/0/url", "loader version");
            return Replace(url);
        }
    }

}