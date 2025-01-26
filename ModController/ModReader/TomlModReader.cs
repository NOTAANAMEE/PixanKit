using PixanKit.ModController.Mod;
using Newtonsoft.Json.Linq;
using PixanKit.ModController.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace PixanKit.ModController.ModReader
{
    /*
     * 1. Read Mod
     * 2. Load File
     */
    public static class TomlModReader
    {
        public static ModFile ReadFile(ModCollection collection, ModReader.ModArchive archive)
        {
            //read the config and get the id
            var Tomlcontent = Toml.Parse(ReadContent(archive.ConfigPath)).ToModel();
            string id = GetID(Tomlcontent);

            //if no cache, load from file
            ModFile modfile;
            if (collection.ModCache.ContainsKey(id))
                modfile = JsonModReader.LoadFromFile(collection,
                    collection.ModCache[id] as JObject ?? [], archive, id);
            else
                modfile = LoadFromFile(collection, Tomlcontent, archive, id);
            
            //get the metadata of the mod, if not, load from file
            if (ModModule.Instance?.ModDatas.TryGetValue(id, out var data) ?? false)
                data.Register(modfile);
            else
            {
                ModMetaData moddata = GetMetaData(Tomlcontent);
                ModModule.Instance?.AddMetaData(moddata);
                moddata.Register(modfile);
            }

            //return
            return modfile;
        }

        /// <summary>
        /// Load the mod from the config file
        /// </summary>
        private static ModFile LoadFromFile(ModCollection collection, TomlTable Tomlcontent,
            ModReader.ModArchive archive, string modid)
        {
            var time = JsonModReader.GetBuildTimeFromManifest(archive);
           

            var modFile = new ModFile()
            {
                Owner = collection,
                Dependencies = GetDependencies(Tomlcontent, modid),
                Version = GetVersionFromConfig(Tomlcontent, archive),
                ReleaseDate = time,
            };
            JsonModReader.CheckDependencies(modFile.Dependencies, archive);
            return modFile;
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
        private static List<string> GetDependencies(TomlTable tomlData, string modid)
        {
            List<string> ret = [];
            var dependencies = (tomlData["dependencies"] as TomlTable)?
                [modid] as TomlArray ?? [];
            foreach (var dependency in dependencies.Cast<TomlTable?>())
            {
                ret.Add(dependency?["modId"]?.ToString() ?? "");
            }
            return ret;
        }

        /// <summary>
        /// This method gets the modid which is the identiy of a mod
        /// </summary>
        /// <param name="tomlData"></param>
        /// <returns></returns>
        private static string GetID(TomlTable tomlData) 
        {
            return (tomlData["mods"] as TomlTable)?["modId"]?.ToString() ?? "";
        }

        /// <summary>
        /// This method gets the release information from the toml config file
        /// </summary>
        private static string GetVersionFromConfig(TomlTable tomlData, 
            ModReader.ModArchive archive) 
        {
            if (tomlData["mods"] is TomlTable modsTable
                && modsTable["version"].ToString() == "${file.jarVersion}")
                return JsonModReader.GetVersionFromManifest(archive);
            string version = (tomlData["mods"] as TomlTable)?
                ["version"].ToString() ?? "";
            return version;
        }

        /// <summary>
        /// This method reads the metadata from the config file
        /// </summary>
        private static ModMetaData GetMetaData(TomlTable TomlData)
        {
            TomlTable mod = TomlData["mods"] as TomlTable 
                ?? throw new Exception();

            string authors = mod["author"]?.ToString() ?? "";
            ModMetaData md = new()
            {
                Name = mod["name"]?.ToString() ?? "",
                ModID = mod["modId"]?.ToString() ?? "",
                Description = mod["description"]?.ToString() ?? "",
                Authors = [authors]
            };
            return md;
        }
    }
}
