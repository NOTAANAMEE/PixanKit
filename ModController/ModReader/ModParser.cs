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
    public static class ModParser
    {
        public static ModFile Parse(string filePath, ModCollection collection)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var archive = new ZipArchive(fs);
            var entry = archive.GetEntry("fabric.mod.json");
            string filename = Path.GetFileName(filePath);
            ModFile modFile;
            if (entry is not null)
                modFile = ParseFabric(filePath, archive, entry, collection);
            else if ((entry = archive.GetEntry("META-INF/mods.toml")) is not null)
                modFile = ParseFML(filePath, archive, entry, collection);
            else if ((entry = archive.GetEntry("mcmod.info")) is not null)
                modFile = ParseFOV(filePath, archive, entry, collection);
            else if (collection.ModCache.ContainsKey(filename))
                modFile = FabricModParser.LoadAllFromJSON(
                    filePath,
                    archive,
                    collection.ModCache[filename] as JObject ?? []);                
            else modFile = new ModFile(filePath)
            {
                Dependencies = [],
                ReleaseDate = DateTime.Now,
                Version = "unknown",
                MetaData = new()
            };
            archive.Dispose();
            fs.Close();
            return modFile;
        }

        private static ModFile ParseFabric(string filepath, ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream = entry.Open();
            StreamReader sr = new(stream);
            var content = sr.ReadToEnd();
            return FabricModParser.ParseJson(content, filepath, collection, archive);
        }

        private static ModFile ParseFML(string filepath, ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream = entry.Open();
            StreamReader sr = new(stream);
            var content = sr.ReadToEnd();
            return FMLModParser.ParseToml(content, filepath, collection, archive);
        }

        private static ModFile ParseFOV(string filepath, ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream = entry.Open();
            StreamReader sr = new(stream);
            var content = sr.ReadToEnd();
            return FOVModParser.ParseJson(content, filepath, collection, archive);
        }


    }
}
