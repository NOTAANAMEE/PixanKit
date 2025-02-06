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
    public static class FabricModParser
    {
        public static ModFile ParseJson(string jsonContent, string filepath, ModCollection modCollection, ZipArchive archive)
        {
            JObject modEntry = JObject.Parse(jsonContent);

            var modID = GetID(modEntry);

            LoadModFile(modCollection, modID,
                modEntry, archive,
                out List<string> dependenciesList,
                out string version, out DateTime releaseDate);
            
            ModMetaData metaData = LoadMetaData(modID, modEntry, archive);
            
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
            ModMetaData? metaData = null;
            lock (ModModule.Instance?.ModDatas ?? new object())
            if (!ModModule.Instance?.ModDatas.TryGetValue(modID, out metaData) ?? false)
            {
                metaData = new ModMetaData
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
                ModModule.Instance?.AddMetaData(metaData);
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

        internal static ModFile LoadAllFromJSON(string filepath, ZipArchive archive, JObject modEntry) 
        {
            var metadata = GetFromModEntry(modEntry);
            ZipArchiveEntry? archiveEntry = archive.GetEntry("META-INF/MANIFEST.MF");
            List<string> dependenciesList = modEntry?.ToObject<List<string>>() ?? [];
            var version = modEntry?["version"]?.ToString() ?? "";
            var releaseDate = DateTime.Parse(modEntry?["release_date"]?.ToString() ?? "");
            var modFile = new ModFile(filepath) { 
                Dependencies = dependenciesList, 
                Version = version, 
                ReleaseDate = releaseDate 
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
