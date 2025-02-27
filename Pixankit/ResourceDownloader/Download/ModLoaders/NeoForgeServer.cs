using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System.Runtime.CompilerServices;

namespace PixanKit.ResourceDownloader.Download.ModLoaders
{
    /// <summary>
    /// Represents a NeoForge mod loader server.
    /// </summary>
    public class NeoForgeServer: ModLoaderServer
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
        public NeoForgeServer(): base("neoforge")
        {
            Mirrors.Add(new OfficialNeoforgeServer());
            UpdateIndex();

        }

        /// <summary>
        /// Represents the official NeoForge mirror server.
        /// </summary>
        public class OfficialNeoforgeServer : ModLoaderMirror
        {
            HttpClient client = new();

            /// <summary>
            /// Inits the instance of the official server.
            /// </summary>
            public OfficialNeoforgeServer()
            {
                BaseURL = "https://maven.neoforged.net";

            }

            private async Task<List<string>> GetBuild(CancellationToken token)
            {
                //Get Later Versions
                var response = await client.GetAsync(
                    "https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/neoforge", token);
                if (token.IsCancellationRequested) return new();
                var content = await response.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested) return new();
                var array = (JObject.Parse(content)["versions"] as JArray ??
                    []).ToObject<List<string>>()//Parse The Content To List<string>
                    ?? throw new Exception("Impossible exception");//This Exception Will Not Be Thrown
                if (token.IsCancellationRequested) return new();

                return array;
            }

            private async Task<List<string>> GetLagacyBuild()
            {
                //Get 1.20.1 Versions
                var response = await client.GetAsync(
                    "https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/forge");
                var content = await response.Content.ReadAsStringAsync();
                var arrayl = (JObject.Parse(content)["versions"] as JArray ??
                    []).ToObject<List<string>>()//Parse The Content To List<string>
                    ?? throw new Exception("Impossible exception");


                return arrayl;
            }

            private JObject Parse(string version)
            {
                return new JObject()
            {
                { "version", version },
                { "url", "https://maven.neoforged.net/releases/net/neoforged/neoforge/" +
                $"{version}/neoforge-{version}-installer.jar" }
            };

            }

            private int FindBuildsStart(List<string> array, string mcpatch, int lft, int rgh)
            {
                if (array == null || array.Count == 0) return -1;
                if (lft > rgh) return -1;
                int current = (lft + rgh) / 2;

                string currentbuild = array[current];
                string currentbuildmcver = currentbuild[..currentbuild.LastIndexOf('.')];

                string beforebuild = array[current - 1];
                string beforebuildmcver = beforebuild[..beforebuild.LastIndexOf('.')];

                Logger.Info("PixanKit.ResourceDownloader", $"Checking: {currentbuildmcver}, Target: {mcpatch}");

                if (currentbuildmcver == mcpatch && beforebuildmcver != mcpatch)
                    return current;


                if (currentbuildmcver.CompareTo(mcpatch) >= 0)
                    return FindBuildsStart(array, mcpatch, lft, current - 1);


                return FindBuildsStart(array, mcpatch, current + 1, rgh);
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
                string patch = GetPatch(mcversion);
                List<string> builds;
                if (mcversion == "1.20.1")
                {
                    builds = await GetLagacyBuild();
                    return GetBuildsObject(builds, "", 0);
                }
                builds = await GetBuild(token);
                return GetBuildsObject(builds, patch,
                    FindBuildsStart(builds, patch, 0, builds.Count));
            }

            /// <inheritdoc/>
            public override Task<string> GetURL(JObject modloaderinf, CancellationToken token)
                => Task.FromResult((modloaderinf["url"] ?? "").ToString());

        }
    }

}
