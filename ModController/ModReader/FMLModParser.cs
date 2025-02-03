using Newtonsoft.Json.Linq;
using PixanKit.ModController.Mod;
using PixanKit.ModController.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using System.Dynamic;
using System.Security.Cryptography;
using System.IO.Compression;

namespace PixanKit.ModController.ModReader
{
    public static class FMLModParser
    {
        static object Locker = new object();

        public static ModFile ParseToml(string tomlContent, ModCollection modCollection, ZipArchive archive)
        {
            var table = Toml.ToModel(tomlContent) ??
                throw new Exception("Failed to parse TOML");

            var mods = table["mods"] as TomlTableArray ??
                throw new Exception("Invalid TOML: No mods found");

            var modEntry = mods[0];

            var modID = GetID(modEntry);

            LoadModFile(modCollection, modID, 
                table, modEntry, archive,
                out List<string> dependenciesList, 
                out string version, out DateTime releaseDate);

            ModMetaData metaData = LoadMetaData(modID, modEntry, archive);

            var modFile = new ModFile()
            {
                Owner = modCollection,
                Version = version,
                Dependencies = dependenciesList,
                ReleaseDate = releaseDate
            };
            metaData.Register(modFile);
            return modFile;
        }

        private static string GetID(TomlTable modEntry)
            => modEntry["modId"]?.ToString() ?? throw new Exception("Missing modId");

        private static ModMetaData LoadMetaData(string modID, TomlTable modEntry, ZipArchive archive)
        {
            ModMetaData? metaData = null;
            if (!ModModule.Instance?.ModDatas.TryGetValue(modID, out metaData) ?? false)
            {
                string logofile = "";
                if (modEntry.ContainsKey("logoFile")) 
                    logofile = modEntry["logoFile"]?.ToString() ?? "";
                metaData = new ModMetaData
                {
                    ModID = modID,
                    Description = modEntry["description"]?.ToString() ?? "",
                    Authors = [modEntry["authors"]?.ToString() ?? ""],
                    ImageCache = LoadIcon(archive, logofile, modID),
                    Name = modEntry["displayName"]?.ToString() ?? ""
                };
            }
            return metaData ?? throw new Exception("Exception avoid null warning");
        }

        private static void LoadModFile(ModCollection modCollection, string modID,
            TomlTable table, TomlTable modEntry, ZipArchive archive,
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
                releaseDate = modData.Value<DateTime?>("release_date") ?? releaseDate;

                if (modData["depends"] is JArray dependsArray)
                    dependenciesList.AddRange(dependsArray.ToObject<List<string>>() ?? []);
                return;
            }

            version = modEntry["version"].ToString() ?? version;

            if (version == "${file.jarVersion}" && archiveEntry is not null)
                version = GetVersionFromManifest(archiveEntry);

            if (!table.ContainsKey("dependencies." + modID)) return;
            if (table["dependencies." + modID] is not TomlArray dependencies) return;

            foreach (var dep in dependencies.Cast<TomlTable>())
            {
                if (dep.TryGetValue("modId", out var modIdObj) && modIdObj is string depModId)
                {
                    dependenciesList.Add(depModId);
                }
            }
            dependenciesList = dependenciesList.Except(GetDependenciesUnderJarJar(archive)).ToList();
        }

        /// <summary>
        /// FUCK U
        /// </summary>
        /// <param name="archive"></param>
        /// <returns></returns>
        internal static string GetVersionFromManifest(ZipArchiveEntry manifestEntry) 
        {
            var filestream = manifestEntry.Open();
            StreamReader sr = new(filestream);
            while (!sr.EndOfStream) 
            {
                var line = sr.ReadLine() ?? "";
                const string impVersionkey = "Implementation-Version: ",
                             speVersionkey = "Specification-Version: ";

                if (line.StartsWith(impVersionkey))
                    return line[impVersionkey.Length..].Trim();

                if (line.StartsWith(speVersionkey))
                    return line[speVersionkey.Length..].Trim();
            }
            return "Unkonwn";
        }

        /// <summary>
        /// FUCK U
        /// </summary>
        /// <param name="archive"></param>
        /// <returns></returns>
        private static List<string> GetDependenciesUnderJarJar(ZipArchive archive)
        {
            List<string> ret = [];
            foreach (var entry in archive.Entries) 
            {
                if (entry.FullName.EndsWith('/')
                    || !entry.FullName.StartsWith("META-INF/jarjar/"))
                    continue;
                try
                {
                    ret.Add(GetEachJarID(entry));
                }
                catch { }
            }
            return ret;
        }

        /// <summary>
        /// FUCK U
        /// </summary>
        /// <param name="archiveEntry"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string GetEachJarID(ZipArchiveEntry archiveEntry)
        {
            var filestream = archiveEntry.Open();
            var archive = new ZipArchive(filestream);
            var entry = archive.GetEntry("META-INF/mods.toml") 
                ?? throw new Exception("Not Invalid Mod");
            var entrystream = entry.Open();
            var streamreader = new StreamReader(entrystream);
            var tomlContent = streamreader.ReadToEnd();
            var table = Toml.ToModel(tomlContent) ??
                throw new Exception("Failed to parse TOML");
            var mods = table["mods"] as TomlTableArray ??
                throw new Exception("Invalid TOML: No mods found");
            var modEntry = mods[0];
            return GetID(modEntry);
        }

        internal static string LoadIcon(ZipArchive archive, string iconPath, string modID)
        {
            if (iconPath == "") return iconPath;
            if (iconPath.StartsWith("http")) return iconPath;
            var entry = archive.GetEntry(iconPath);
            if (entry == null) return iconPath;
            string path = $"{ModModule.IconCachePath}/{modID}" +
                $"{Path.GetExtension(entry.FullName)}";
            //Directory.CreateDirectory(ModModule.IconCachePath);
            lock (Locker)
            {
                if (File.Exists(path)) File.Delete(path);
                entry.ExtractToFile(path);
            }

            return path;
        }
    }
}
