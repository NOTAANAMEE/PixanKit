using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.ModController.Mod;
using PixanKit.ModController.Module;
using System.IO.Compression;

namespace PixanKit.ModController.ModReader
{
    /// <summary>
    /// The config parser class for the old version(1.12.x) Forge mod files.
    /// </summary>
    public static class FOVModParser
    {
        #region Logic
        /// <summary>
        /// This method parses the json config of the Forge mod file and read the data to
        /// generate <see cref="ModFile"/> instance.
        /// </summary>
        /// <param name="jsonContent">The content of the json config</param>
        /// <param name="filepath">The path of the mod file</param>
        /// <param name="modCollection">The mod collection that the mod file belongs to</param>
        /// <param name="archive">The zip archive of the mod file</param>
        /// <returns>ModFile instance represents the mod file</returns>
        /// <exception cref="Exception">json config is not valid</exception>
        public static ModFile ParseJson(string jsonContent, string filepath, ModCollection modCollection, ZipArchive archive)
        {
            if (ModModule.Instance == null)
                throw new InvalidOperationException();

            var modArray = JArray.Parse(jsonContent);
            if (modArray.Count == 0)
                throw new Exception("Invalid JSON: No mod data found");

            var modEntry = (JObject)modArray[0];
            var modID = GetID(modEntry);

            LoadModFile(modCollection, modID,
                modEntry, archive,
                out List<string> deplist,
                out string version, out DateTime releaseDate);

            ModMetaData metaData = LoadMetaData(modID, modEntry, archive);

            var modFile = new ModFile(filepath)
            {
                Owner = modCollection,
                Version = version,
                Dependencies = deplist,
                ReleaseDate = releaseDate
            };
            metaData.Register(modFile);
            return modFile;
        }

        private static string GetID(JObject modEntry)
            => modEntry.GetValue(Format.ToString, "modid");

        private static ModMetaData LoadMetaData(string modID, JObject modEntry, ZipArchive archive)
        {
            if (ModModule.Instance == null) throw new InvalidOperationException();

            if (!ModModule.Instance.ModDatas.TryGetValue(modID, out ModMetaData? metaData))
            {
                metaData = new ModMetaData
                {
                    ModID = modID,
                    Description = modEntry.GetDescription(),
                    Authors = modEntry.GetAuthors(),
                    ImageCache = FMLModParser.
                        LoadIcon(archive, modEntry.GetIcon(), modID),
                    Name = modEntry.GetName()
                };
                ModModule.Instance?.AddMetaData(metaData);
            }
            return metaData ?? throw new Exception("Exception avoid null warning");
        }

        private static void LoadModFile(ModCollection modCollection, string modID,
            JObject modEntry, ZipArchive archive,
            out List<string> depList, out string version, out DateTime releaseDate
            )
        {
            var entry = archive.GetEntry("META-INF/MANIFEST.MF");
            releaseDate = entry?.LastWriteTime.UtcDateTime ?? DateTime.UtcNow;
            modCollection.ModCache.TryGetValue(Format.ToJObject, modID, out var modData);

            if (modData == null)
            {
                version = modEntry.GetVersion();
                depList = modEntry.GetDataDeps();
                return;
            }
            FabricModParser.ReadFromCache(modData, releaseDate,
                out depList, out version, out releaseDate);
        }
        #endregion

        #region JSONReader
        private static string GetDescription(this JObject modEntry)
            => modEntry.GetOrDefault(Format.ToString, "description", "");

        private static string[] GetAuthors(this JObject modEntry)
            => modEntry["authors"]?.Select(a => a.ToString()).ToArray() ?? [];

        private static string GetName(this JObject modEntry)
            => modEntry.GetOrDefault(Format.ToString, "name", "");

        private static string GetVersion(this JObject modCache)
            => modCache.GetOrDefault(Format.ToString, "version", "");

        private static string GetIcon(this JObject modEntry)
            => modEntry.GetOrDefault(Format.ToString, "logoFile", "");

        private static List<string> GetCacheDeps(this JObject modCache)
            => modCache.GetOrDefault(Format.ToJArray, "depends", []).ToList(Format.ToString);

        private static List<string> GetDataDeps(this JObject modData)
            => modData.GetOrDefault(Format.ToJArray, "requiredMods", []).ToList(Format.ToString);
        #endregion

    }
}
