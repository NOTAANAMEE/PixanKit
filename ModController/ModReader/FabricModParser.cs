using Newtonsoft.Json.Linq;
using PixanKit.ModController.Mod;
using PixanKit.ModController.Module;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModController.ModReader
{
    /// <summary>
    /// The config parser class for the Fabric mod files.
    /// </summary>
    public static class FabricModParser
    {
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
            JObject modEntry = JObject.Parse(jsonContent);

            var modID = GetID(modEntry);

            LoadModFile(modCollection, modID,
                modEntry, archive,
                out List<string> dependenciesList,
                out string version, out DateTime releaseDate);

            ModMetaData metaData; 

            lock (ModModule.Instance.MetaDataLocker)
                metaData = LoadMetaData(modID, modEntry, archive);

            var modFile = new ModFile(filepath)
            {
                Owner = modCollection,
                Version = version,
                Dependencies = dependenciesList,
                ReleaseDate = releaseDate
            };
            metaData.Register(modFile);
            return modFile;
        }

        private static string GetID(JObject modEntry)
            => modEntry["id"]?.ToString() ?? throw new Exception("Missing modId");

        private static ModMetaData LoadMetaData(string modID, JObject modEntry, ZipArchive archive)
        {
            if (ModModule.Instance == null) throw new InvalidOperationException("Exception");
            var idInCache = ModModule.Instance.ModDatas.TryGetValue(modID, 
                out ModMetaData? metaData);

            if (!idInCache)
            {
                metaData = new ModMetaData()
                {
                    ModID = modID,
                    Description = modEntry["description"]?.ToString() ?? "",
                    Authors =
                    modEntry["authors"]?.Select(a => a.ToString()).ToArray() ?? [],
                    ImageCache =
                        FMLModParser.
                        LoadIcon(archive, modEntry["icon"]?.ToString() ?? "", modID),
                    Name = modEntry["name"]?.ToString() ?? ""
                };
                ModModule.Instance.AddMetaData(metaData);
            }
            return metaData ?? throw new Exception("Exception avoid null warning");
        }

        private static void LoadModFile(ModCollection modCollection, string modID,
            JObject modEntry, ZipArchive archive,
            out List<string> dependenciesList, out string version, out DateTime releaseDate
            )
        {
            JObject? modData;
            ZipArchiveEntry? archiveEntry = archive.GetEntry("META-INF/MANIFEST.MF");
            dependenciesList = [];
            version = "";
            releaseDate = archiveEntry?.LastWriteTime.UtcDateTime
                ?? DateTime.UtcNow;

            if ((modData = modCollection.ModCache[modID] as JObject) != null)
            {
                version = modData["version"]?.ToString() ?? version;
                string? releaseDatestr = modData["release_date"]?.ToString();
                if (releaseDatestr != null)
                    releaseDate = DateTime.Parse(releaseDatestr);

                if (modData["depends"] is JArray dependsArray)
                    dependenciesList.AddRange(dependsArray.ToObject<List<string>>() ?? []);
                return;
            }

            version = modEntry["version"]?.ToString() ?? version;

            foreach (var item in modEntry["depends"] as JObject ?? [])
                dependenciesList.Add(item.Key);
        }

        internal static ModMetaData GetFromModEntry(JObject modEntry)
        {
            var id = modEntry["id"]?.ToString() ?? "";
            if (ModModule.Instance == null) 
                throw new InvalidOperationException("ModModule Not Inited Yet");
            return ModModule.Instance.ModDatas[id];
        }

        internal static ModFile LoadAllFromJSON(string filepath, JObject modEntry) 
        {
            var metadata = GetFromModEntry(modEntry);
            List<string> dependenciesList = modEntry?.ToObject<List<string>>() ?? [];
            var version = modEntry?["version"]?.ToString() ?? "";
            var releaseDate = DateTime.Parse(modEntry?["release_date"]?.ToString() ?? "");
            var modFile = new ModFile(filepath) { 
                Dependencies = dependenciesList, 
                Version = version, 
                ReleaseDate = releaseDate,
                ValidStructure = false,
            };
            metadata.Register(modFile);
            return modFile;            
        }

        internal static ModMetaData ParseModMetaDataFromJSON(JObject modEntry) 
        {
            return new ModMetaData()
            {
                ModID = modEntry["id"]?.ToString() ?? "",
                Description = modEntry["description"]?.ToString() ?? "",
                Authors =
                    modEntry["authors"]?.Select(a => a.ToString()).ToArray() ?? [],
                ImageCache = modEntry["icon"]?.ToString() ?? "",
                Name = modEntry["name"]?.ToString() ?? ""
            };
        }
    }
}
