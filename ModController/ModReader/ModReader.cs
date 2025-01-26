using PixanKit.ModController.Mod;
using PixanKit.LaunchCore.Extention;
using PixanKit.ModController.Module;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModController.ModReader
{
    public static class ModReader
    {
        public record class ModArchive(ZipArchive Archive, string ConfigPath, FileStream Stream, bool JSON);

        public static ModFile ReadFile(ModCollection collection, string filepath)
        {
            var archive = Open(filepath);
            ModFile mod;
            if(archive.JSON) mod =  JsonModReader.ReadFile(collection, archive);
            else mod =  TomlModReader.ReadFile(collection, archive);
            Free(archive);
            return mod;
        }

        internal static ModArchive Open(string filepath) 
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException();
            FileStream fs = new(filepath, FileMode.Open);
            ZipArchive ziparchive = new(fs);
            string configpath = $"{Files.CacheDir}/{filepath.GetHashCode()}";
            var entry = ziparchive.GetEntry("fabric.mod.json");
            bool json = true;
            if (entry != null) 
                entry.ExtractToFile(configpath += ".json");
            else
            {
                entry = ziparchive.GetEntry("META-INF/mods.toml") ?? 
                    throw new Exception("Invalid Mod");
                entry.ExtractToFile(configpath += ".toml");
                json = false;
            }
            return new(ziparchive, configpath, fs, json);
        }

        internal static void Free(ModArchive archive) 
        {
            archive.Archive.Dispose();
            archive.Stream.Close();
            File.Delete(archive.ConfigPath);
        }

        internal static void UpdateIconCache(ModArchive archive, string ID, string iconpath)
        {
            if (iconpath.StartsWith("https://") || iconpath.StartsWith("https://"))
                return;
            archive.Archive.GetEntry(iconpath)?
                .ExtractToFile($"{ModModule.IconCachePath}/" +
                $"{ID}.{iconpath[iconpath.LastIndexOf('.')..]}");
        }
    }
}
