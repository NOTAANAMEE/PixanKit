using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;
using PixanKit.LaunchCore.Logger;

namespace PixanKit.ResourceDownloader.Download.ModLoaders;

/// <summary>
/// Represents a NeoForge mod loader server.
/// </summary>
public class NeoForgeServer : ModLoaderServer
{
    /// <summary>
    /// Initor. Dont touch it
    /// </summary>
    [ModuleInitializer]
    public static void Init()
    {
        _ = new NeoForgeServer();
    }

    /// <summary>
    /// Initor
    /// </summary>
    public NeoForgeServer() : base("neoforge")
    {
        Mirrors.Add(new OfficialNeoforgeServer());
        UpdateIndex();

    }

    /// <summary>
    /// Represents the official NeoForge mirror server.
    /// </summary>
    public class OfficialNeoforgeServer : ModLoaderMirror
    {
        private readonly HttpClient _client = new();

        /// <summary>
        /// Inits the instance of the official server.
        /// </summary>
        public OfficialNeoforgeServer()
        {
            BaseUrl = "https://maven.neoforged.net";

        }

        private async Task<List<string>> GetBuild(CancellationToken token)
        {
            //Get Later Versions
            var response = await _client.GetAsync(
                "https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/neoforge", token);
            if (token.IsCancellationRequested) return [];
            var content = await response.Content.ReadAsStringAsync(token);
            if (token.IsCancellationRequested) return [];
            var array = (JObject.Parse(content)["versions"] as JArray ??
                         []).ToObject<List<string>>()//Parse The Content To List<string>
                        ?? throw new Exception("Impossible exception");//This Exception Will Not Be Thrown
            if (token.IsCancellationRequested) return [];

            return array;
        }

        private async Task<List<string>> GetLegacyBuild()
        {
            //Get 1.20.1 Versions
            var response = await _client.GetAsync(
                "https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/forge");
            var content = await response.Content.ReadAsStringAsync();
            var versionList = (JObject.Parse(content)["versions"] as JArray ??
                          []).ToObject<List<string>>()//Parse The Content To List<string>
                         ?? throw new Exception("Impossible exception");


            return versionList;
        }

        /*private JObject Parse(string version)
        {
            return new JObject()
            {
                { "version", version },
                { "url", "https://maven.neoforged.net/releases/net/neoforged/neoforge/" +
                         $"{version}/neoforge-{version}-installer.jar" }
            };

        }*/

        private int FindBuildsStart(List<string> array, string gamePatch, int lft, int rgh)
        {
            if (array.Count == 0) return -1;
            if (lft > rgh) return -1;
            var current = (lft + rgh) / 2;

            var currentBuild = array[current];
            var currentBuildGameVer = currentBuild[..currentBuild.LastIndexOf('.')];

            var beforeBuild = array[current - 1];
            var beforeBuildGameVer = beforeBuild[..beforeBuild.LastIndexOf('.')];

            Logger.Info("PixanKit.ResourceDownloader", $"Checking: {currentBuildGameVer}, Target: {gamePatch}");

            if (currentBuildGameVer == gamePatch && beforeBuildGameVer != gamePatch)
                return current;


            return string.Compare(currentBuildGameVer, gamePatch, StringComparison.Ordinal) >= 0 ? FindBuildsStart(array, gamePatch, lft, current - 1) : FindBuildsStart(array, gamePatch, current + 1, rgh);
        }

        /// <summary>
        /// The method gets the patch version number of Minecraft
        /// For example: 1.20.1 it will return 20.1
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static string GetPatch(string version)
            => version[(version.IndexOf('.') + 1)..];

        private static JArray GetBuildsObject(List<string> versions, string patch, int start)
        {
            JArray builds = [];
            while (++start < versions.Count && versions[start].StartsWith(patch))
            {
                builds.Add(
                    new JObject()
                    {
                        { "version", versions[start] },
                        { "url",
                            "https://maven.neoforged.net/releases/net/neoforged/neoforge/" +
                            versions[start] +
                            $"/neoforge-{versions[start]}-installer.jar"}
                    });
            }
            return builds;
        }

        /// <inheritdoc/>
        public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
        {
            if (mcversion == "1.20.1") return true;//The First Supported Minecraft Of NeoForge
            if (!mcversion.Contains('.')) return false; //No Snapshot Builds
            var builds = await GetBuild(token);
            return FindBuildsStart(builds, GetPatch(mcversion), 0, builds.Count - 1)
                   != -1;
        }

        /// <inheritdoc/>
        public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
        {
            var patch = GetPatch(mcversion);
            List<string> builds;
            if (mcversion == "1.20.1")
            {
                builds = await GetLegacyBuild();
                return GetBuildsObject(builds, "", 0);
            }
            builds = await GetBuild(token);
            return GetBuildsObject(builds, patch,
                FindBuildsStart(builds, patch, 0, builds.Count));
        }

        /// <inheritdoc/>
        public override Task<string> GetUrl(JObject modloaderinf, CancellationToken token)
            => Task.FromResult((modloaderinf["url"] ?? "").ToString());

    }
}