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
            if (entry is not null)
                return ParseFabric(archive, entry, collection);
            else if ((entry = archive.GetEntry("META-INF/mods.toml")) is not null)
                return ParseFML(archive, entry, collection);
            else if ((entry = archive.GetEntry("mcmod.info")) is not null)
                return ParseFOV(archive, entry, collection);
            else return new ModFile()
            {
                Dependencies = [],
                ReleaseDate = DateTime.Now,
                Version = "unknown",
                MetaData = new()
            };
        }

        private static ModFile ParseFabric(ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream = entry.Open();
            StreamReader sr = new(stream);
            var content = sr.ReadToEnd();
            return FabricModParser.ParseJson(content, collection, archive);
        }

        private static ModFile ParseFML(ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream = entry.Open();
            StreamReader sr = new(stream);
            var content = sr.ReadToEnd();
            return FMLModParser.ParseToml(content, collection, archive);
        }

        private static ModFile ParseFOV(ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream = entry.Open();
            StreamReader sr = new(stream);
            var content = sr.ReadToEnd();
            return FOVModParser.ParseJson(content, collection, archive);
        }


    }
}
