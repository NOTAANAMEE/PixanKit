using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tomlyn.Model;
using Tomlyn;
using PixanKit.LaunchCore;
using PixanKit.ModController.Module;
using PixanKit.ModController.Mod;

namespace PixanKit.ModController.ModReader
{
    public static class JsonModReader
    {
        public static ModFile ReadFile(ModCollection collection, ModReader.ModArchive archive)
        {
            //read the config and get the id
            var JsonContent = JObject.Parse(ReadContent(archive.ConfigPath));
            string id = GetID(JsonContent);


            ModFile modfile;
            //if no cache, load from file
            if (collection.ModCache.ContainsKey(id))
                modfile = LoadFromFile(collection,
                    collection.ModCache[id] as JObject ?? [], archive, id);
            else
                modfile = LoadFromFile(collection, JsonContent, archive, id);

            //get the metadata of the mod, if not, load from file
            if (ModModule.Instance?.ModDatas.TryGetValue(id, out var data) ?? false)
                data.Register(modfile);
            else
            {
                ModMetaData moddata = GetMetaData(JsonContent);
                ModModule.Instance?.AddMetaData(moddata);
                moddata.Register(modfile);
            }

            //return
            return modfile;
        }

        /// <summary>
        /// Load the mod from the config file
        /// </summary>
        internal static ModFile LoadFromFile(ModCollection collection, JObject jsonData,
            ModReader.ModArchive archive, string modid)
        {
            DateTime date;
            if (jsonData["release"] != null)
                date = DateTime.Parse(jsonData["release"]?.ToString() ?? "");
            else
                date = GetBuildTimeFromManifest(archive);

            var modFile = new ModFile()
            {
                Owner = collection,
                Dependencies = GetDependencies(jsonData, modid),
                Version = GetVersionFromConfig(jsonData, archive),
                ReleaseDate = date,
            };
            CheckDependencies(modFile.Dependencies, archive);
            return modFile;
        }

        internal static DateTime GetBuildTimeFromManifest(ModReader.ModArchive archive)
        {
            return archive.Archive.GetEntry("META-INF/MANIFEST.MF")?
                .LastWriteTime.DateTime ??
                throw new InvalidOperationException(
                    "Invalid JAR file, No META-INF");
        }

        /// <summary>
        /// Read the content from file path
        /// </summary>
        private static string ReadContent(string filepath)
        {
            return File.ReadAllText(filepath);
        }

        /// <summary>
        /// Read the dependencies from the config file
        /// </summary>
        private static List<string> GetDependencies(JObject jsonData, string modid)
        {
            List<string> dependencies = [];
            foreach (var a in jsonData["depends"] as JObject ?? [])
                dependencies.Add(a.Key);
            return dependencies;
        }

        /// <summary>
        /// This method gets the modid which is the identiy of a mod
        /// </summary>
        private static string GetID(JObject jsonData)
        {
            return jsonData["id"]?.ToString() ?? "";
        }

        /// <summary>
        /// This method remove the dependencies that already existed in jar files
        /// </summary>
        internal static void CheckDependencies(List<string> dependencies, ModReader.ModArchive archive)
        {
            var zipentries = archive.Archive.Entries.
                Where(a => a.FullName.StartsWith("META-INF/jarjar")).ToArray();
            List<string> remove = [];
            foreach (var dependency in dependencies)
            {
                if (dependency == "minecraft" ||
                    ModModule.ModLoaders.Contains(dependency))
                    continue;
                if (zipentries.Any(
                    a => a.FullName.EndsWith(".jar") && a.FullName.Contains(dependency)))
                    remove.Add(dependency);
            }
            foreach (var removeitem in remove)
            {
                dependencies.Remove(removeitem);
            }
        }

        /// <summary>
        /// This method gets the release information from the toml config file
        /// </summary>
        private static string GetVersionFromConfig(JObject jsonData,
            ModReader.ModArchive archive)
        {
            if (jsonData["version"]?.ToString() == "${file.jarVersion}")
                return GetVersionFromManifest(archive);
            string version = jsonData["version"]?.ToString() ?? "";
            return version;
        }

        /// <summary>
        /// Ths method gets the release information from the Manifest file
        /// </summary>
        internal static string GetVersionFromManifest(ModReader.ModArchive archive)
        {
            var manifestEntry = archive.Archive.GetEntry("META-INF/MANIFEST.MF")
                ?? throw new Exception();
            var reader = new StreamReader(manifestEntry.Open());
            string version = "";
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine() ?? "";
                if (line.StartsWith("Implementation-Version:"))
                    version = line["Implementation-Version:".Length..];
            }
            return version;
        }

        /// <summary>
        /// This method reads the metadata from the config file
        /// </summary>
        internal static ModMetaData GetMetaData(JObject jsonData)
        {
            ModMetaData md = new()
            {
                Name = jsonData["name"]?.ToString() ?? "",
                ModID = jsonData["id"]?.ToString() ?? "",
                Description = jsonData["description"]?.ToString() ?? "",
                Authors = jsonData["authors"]?.Select(a => a.ToString()).ToArray() ?? [],
            };
            return md;
        }
    }
}
