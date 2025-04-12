using PixanKit.LaunchCore.Json;
using PixanKit.ModController.Mod;
using PixanKit.ModController.Module;
using System.IO.Compression;
using Tomlyn;
using Tomlyn.Model;

namespace PixanKit.ModController.ModReader
{
    /// <summary>
    /// The config parser class for the latest(1.13.x-current) Forge and NeoForge mod files.
    /// </summary>
    public static class FMLModParser
    {
        static readonly object IconLocker = new();

        #region Logic
        /// <summary>
        /// This method parses the toml config of the Forge mod file and read the data to
        /// generate <see cref="ModFile"/> instance.
        /// </summary>
        /// <param name="tomlContent">The content of the toml config</param>
        /// <param name="filepath">The path of the mod file</param>
        /// <param name="modCollection">The mod collection that the mod file belongs to</param>
        /// <param name="archive">The zip archive of the mod file</param>
        /// <returns>ModFile instance represents the mod file</returns>
        /// <exception cref="Exception">toml config is not valid</exception>
        public static ModFile ParseToml(string tomlContent, string filepath, ModCollection modCollection, ZipArchive archive)
        {
            _ = ModModule.Instance ??
                throw new InvalidOperationException("ModModule has not being inited");

            var table = Toml.ToModel(tomlContent) ??
                throw new Exception("Failed to parse TOML");
            var mods = table.GetValue<TomlTableArray>("mods");

            var modEntry = mods[0];
            var modID = GetID(modEntry);

            LoadModFile(modCollection, modID,
                table, modEntry, archive,
                out List<string> depList,
                out string version, out DateTime releaseDate);

            ModMetaData metaData = LoadMetaData(modID, modEntry, archive);

            var modFile = new ModFile(filepath)
            {
                Owner = modCollection,
                Version = version,
                Dependencies = depList,
                ReleaseDate = releaseDate
            };
            metaData.Register(modFile);
            return modFile;
        }

        private static string GetID(TomlTable modEntry)
            => modEntry["modId"]?.ToString() ?? throw new Exception("Missing modId");

        private static ModMetaData LoadMetaData(string modID, TomlTable modEntry, ZipArchive archive)
        {
            if (ModModule.Instance == null) throw new InvalidOperationException();
            if (!ModModule.Instance.ModDatas.TryGetValue(modID, out ModMetaData? metaData))
            {
                string logofile = modEntry.GetIcon();
                metaData = new ModMetaData
                {
                    ModID = modID,
                    Description = modEntry.GetDescription(),
                    Authors = [modEntry.GetOrDefault("authors", "")],
                    ImageCache = LoadIcon(archive, logofile, modID),
                    Name = modEntry.GetOrDefault("displayName", "")
                };
                ModModule.Instance?.AddMetaData(metaData);
            }

            return metaData ?? throw new Exception("Exception avoid null warning");
        }

        private static void LoadModFile(ModCollection modCollection, string modID,
            TomlTable table, TomlTable modEntry, ZipArchive archive,
            out List<string> depList, out string version, out DateTime releaseDate
            )
        {
            var entry = archive.GetEntry("META-INF/MANIFEST.MF");
            releaseDate = entry?.LastWriteTime.UtcDateTime ?? DateTime.UtcNow;
            modCollection.ModCache.TryGetValue(Format.ToJObject, modID, out var modData);

            if (modData == null)
            {
                version = modEntry.GetVersion();
                if (version == "${file.jarVersion}" && entry is not null)
                    version = GetVersionFromManifest(entry);
                depList = modEntry.GetDeps(modID);
                depList = [.. depList.Except(GetDependenciesUnderJarJar(archive))];
                return;
            }

            FabricModParser.ReadFromCache(modData, releaseDate,
                    out depList, out version, out releaseDate);
        }

        /// <summary>
        /// FUCK U
        /// </summary>
        /// <param name="manifestEntry"></param>
        /// <returns></returns>
        internal static string GetVersionFromManifest(ZipArchiveEntry manifestEntry)
        {
            var filestream = manifestEntry.Open();
            StreamReader sr = new(filestream);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine() ?? "";
                const string impVerkey = "Implementation-Version: ",
                             speVerkey = "Specification-Version: ";

                if (line.StartsWith(impVerkey))
                    return line[impVerkey.Length..].Trim();

                if (line.StartsWith(speVerkey))
                    return line[speVerkey.Length..].Trim();
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
                if (!CheckFile(entry.FullName)) continue;
                try { ret.Add(GetEachJarID(entry)); }
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
            var entry = archive.GetEntry("META-INF/mods.toml") ??
                               throw new Exception("Not Invalid Mod");
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
            var path = $"{ModModule.IconCachePath}/{modID}" +
                $"{Path.GetExtension(entry.FullName)}";

            lock (IconLocker)
            {
                if (File.Exists(path)) File.Delete(path);
                entry.ExtractToFile(path);
            }

            return path;
        }
        #endregion

        #region Check
        private static bool CheckFile(string path)
            => path.StartsWith("META-INF/jarjar/") && !path.EndsWith('/');
        #endregion

        #region TOMLReader
        private static string GetVersion(this TomlTable table)
            => table["version"].ToString() ?? "";

        private static List<string> GetDeps(this TomlTable table, string modID)
        {
            var depList = new List<string>();
            if (!table.ContainsKey("dependencies." + modID)) return depList;
            if (table["dependencies." + modID] is not TomlArray dependencies)
                return depList;

            foreach (var dep in dependencies.Cast<TomlTable>())
            {
                if (dep.TryGetValue("modId", out var modIdObj) && modIdObj is string depModId)
                {
                    depList.Add(depModId);
                }
            }
            return depList;
        }

        private static string GetIcon(this TomlTable table)
            => table.GetOrDefault("logoFile", "");

        private static string GetDescription(this TomlTable table)
            => table.GetOrDefault("description", "");
        #endregion
    }
}
