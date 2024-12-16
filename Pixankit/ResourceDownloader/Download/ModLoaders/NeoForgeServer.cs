using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.ModLoaders
{
    /// <summary>
    /// Represents a NeoForge mod loader server.
    /// </summary>
    public class NeoForgeServer: ModLoaderServer
    {
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
                    new JArray()).ToObject<List<string>>();//Parse The Content To List<string>
                if (array == null)
                    throw new Exception("Impossible exception");//This Exception Will Not Be Thrown
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
                    new JArray()).ToObject<List<string>>();//Parse The Content To List<string>

                if (arrayl == null) throw new Exception("Impossible exception");


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

            /// <inheritdoc/>
            public override async Task<bool> CheckBuild(string mcversion, CancellationToken token)
            {
                if (mcversion == "1.20.1") return true;//The First Supported Minecraft Of NeoForge
                if (!mcversion.Contains('.')) return false; //No Snapshot Builds
                var builds = await GetBuild(token);
                if (token.IsCancellationRequested) return false;
                var version = mcversion.Split('.');
                int minor = int.Parse(version[1]), patch = int.Parse(
                    version.ElementAtOrDefault(2) ?? "0");
                //Get Minor Version and Patch Version
                foreach (var build in builds)
                {
                    if (token.IsCancellationRequested) return false;
                    var nfversion = mcversion.Split('.');
                    int nfminor = int.Parse(version[1]), nfpatch = int.Parse(version[2]);
                    if (nfminor == minor && nfpatch == patch) return true;
                    else if (nfminor > minor || nfpatch > patch) return false;
                    //Arrange In Order From Smallest To Largest
                }
                return false;
            }

            /// <inheritdoc/>
            public override async Task<JArray> GetBuild(string mcversion, CancellationToken token)
            {
                var version = mcversion.Split('.');
                int minor = int.Parse(version[1]), patch = int.Parse(
                    version.ElementAtOrDefault(2) ?? "0");
                List<string> builds;
                if (mcversion == "1.20.1") builds = await GetLagacyBuild();
                else builds = await GetBuild(token);
                if (token.IsCancellationRequested) return new JArray();
                JArray ret = new();
                foreach (string build in builds)
                {
                    if (token.IsCancellationRequested) return new JArray();
                    var nfversion = mcversion.Split('.');
                    int nfminor = int.Parse(version[1]), nfpatch = int.Parse(version[2]);
                    if (nfminor == minor && nfpatch == patch)
                        ret.Add(build);
                    else if (nfminor > minor || nfpatch > patch)
                        break;
                }
                return ret;
            }

            /// <inheritdoc/>
            public override Task<string> GetURL(JObject modloaderinf, CancellationToken token)
                => Task.FromResult((modloaderinf["url"] ?? "").ToString());

        }
    }

}
