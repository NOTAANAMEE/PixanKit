using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.ModController.Mod;
using PixanKit.ModController.Module;
using System.Data;
using System.IO.Compression;

namespace PixanKit.ModController.ModReader
{
    /// <summary>
    /// The config parser class for the Fabric mod files.
    /// </summary>
    public static class FabricModParser
    {
        #region Logic
        /// <summary>
        /// This method parses the json config of the Fabric mod file and read the data to
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
            var modEntry = JObject.Parse(jsonContent);
            var modID    = GetID(modEntry);

            LoadModFile(modCollection, modID,
                modEntry, archive,
                out List<string> dependenciesList,
                out string version, out DateTime releaseDate);

            ModMetaData metaData;
            if (ModModule.Instance == null) 
                throw new InvalidOperationException("Init ModModule first!");
            lock (ModModule.Instance.MetaDataLocker)
                metaData = LoadMetaData(modID, modEntry, archive);

            var modFile = new ModFile(filepath)
            {
                Owner        = modCollection,
                Version      = version,
                Dependencies = dependenciesList,
                ReleaseDate  = releaseDate
            };
            metaData.Register(modFile);
            return modFile;
        }

        private static string GetID(JObject modEntry)
            => modEntry.GetValue(Format.ToString, "id");

        private static ModMetaData LoadMetaData(string modID, JObject modEntry, ZipArchive archive)
        {
            if (ModModule.Instance == null) 
                throw new InvalidOperationException("Exception");

            var idInCache = ModModule.Instance.ModDatas.TryGetValue(modID, 
                out ModMetaData? metaData);

            if (idInCache) return metaData ?? 
                    throw new Exception("Exception avoid null warning");

            metaData = new ModMetaData()
            {
                ModID       = modID,
                Description = modEntry.GetDescription(),
                Authors     = modEntry.GetAuthors(),
                Name        = modEntry.GetName(),
                ImageCache  = FMLModParser.LoadIcon(archive, modEntry.GetIcon(), modID),
            };
            ModModule.Instance.AddMetaData(metaData);

            return metaData ?? throw new Exception("Exception avoid null warning");
        }

        private static void LoadModFile(ModCollection modCollection, string modID,
            JObject modEntry, ZipArchive archive,
            out List<string> depList, out string version, out DateTime releaseDate
            )
        {
            var entry   = archive.GetEntry("META-INF/MANIFEST.MF");
            releaseDate = entry?.LastWriteTime.UtcDateTime ?? DateTime.UtcNow;
            modCollection.ModCache.TryGetValue(Format.ToJObject, modID, out var modData);

            if (modData == null)
            {
                version = modEntry.GetVersion();
                depList = modEntry.GetDataDeps();
                return;
            }

            modData.ReadFromCache(releaseDate, out depList, out version, out releaseDate);
        }

        internal static ModMetaData GetFromModEntry(JObject modEntry)
        {
            if (ModModule.Instance == null)
                throw new InvalidOperationException("ModModule Not Inited Yet");

            return ModModule.Instance.ModDatas[GetID(modEntry)];
        }

        internal static ModFile LoadAllFromJSON(string filepath, JObject modEntry) 
        {
            var metadata       = GetFromModEntry(modEntry);
            var depList        = modEntry.GetCacheDeps();
            var version        = modEntry.GetVersion();
            var releaseDate    = modEntry.GetValue(Format.ToDateTime,"release_date");
            var modFile        = new ModFile(filepath) { 
                Dependencies   = depList, 
                Version        = version, 
                ReleaseDate    = releaseDate,
                ValidStructure = false,
            };
            metadata.Register(modFile);
            return modFile;            
        }

        internal static ModMetaData ParseModMetaDataFromJSON(JObject modEntry) 
        {
            return new ModMetaData()
            {
                ModID       = GetID(modEntry),
                Description = modEntry.GetDescription(),
                Authors     = modEntry.GetAuthors(),
                ImageCache  = modEntry.GetOrDefault(Format.ToString, "icon", ""),
                Name        = modEntry.GetName(),
            };
        }

        internal static void ReadFromCache(this JObject modData, DateTime releaseDate, 
            out List<string> depList, out string ver, out DateTime rlsDate)
        {
            ver     = modData.GetVersion();
            rlsDate = modData.GetOrDefault(Format.ToDateTime,
                      "release_date", releaseDate);
            depList = modData.GetCacheDeps();
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
            => modEntry.GetOrDefault(Format.ToString, "icon", "");

        private static List<string> GetCacheDeps(this JObject modCache)
            => modCache.GetOrDefault(Format.ToJArray, "depends", []).ToList(Format.ToString);

        private static List<string> GetDataDeps(this JObject modData)
        {
            List<string> deps = [];
            JObject arr = modData.GetOrDefault(Format.ToJObject, "depends", []);
            foreach (var pair in arr)
                deps.Add(pair.Key);
            return deps;
        }
        #endregion
    }
}
