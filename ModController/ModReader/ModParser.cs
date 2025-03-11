using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Log;
using PixanKit.ModController.Mod;
using PixanKit.ModController.Module;
using System.IO.Compression;

namespace PixanKit.ModController.ModReader
{
    /// <summary>
    /// The delegate that parses the mod file from the params.
    /// </summary>
    /// <param name="filepath">The path of the file</param>
    /// <param name="archive">The <see cref="ZipArchive"/> of the file</param>
    /// <param name="entry">The entry of the config</param>
    /// <param name="collection">The <see cref="ModCollection"/> that the
    /// file belongs to</param>
    /// <returns>Returns the <see cref="ModFile"/> instance represents the file</returns>
    public delegate ModFile ModParserFunc(string filepath, 
        ZipArchive archive, 
        ZipArchiveEntry entry,
        ModCollection collection);

    /// <summary>
    /// This static class parses the mod from the mod file.
    /// </summary>
    public static class ModParser
    {
        /// <summary>
        /// The info path and matched parsing function
        /// </summary>
        public static readonly List<KeyValuePair<string, ModParserFunc>> ModParsers =
        [
            new("fabric.mod.json", ParseFabric),
            new("META-INF/mods.toml", ParseFML),
            new("mcmod.info", ParseFOV),
        ];

        /// <summary>
        /// Get the information of the mod from the file and the owner.
        /// </summary>
        /// <param name="filePath">The path of the mod file</param>
        /// <param name="collection">The owner of the mod</param>
        /// <returns>A ModFile instance which represents the mod file</returns>
        public static ModFile Parse(string filePath, ModCollection collection)
        {
            var fs           = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var archive      = new ZipArchive(fs);
            ModFile? modFile = null;

            foreach (var item in ModParsers)
            {
                var entry = archive.GetEntry(item.Key);
                if (entry == null) continue;
                modFile   = item.Value(filePath, archive, entry, collection);
                break;
            }

            modFile ??= ParseInv(filePath, collection);
            archive.Dispose();
            fs.Close();
            return modFile;
        }

        private static ModFile ParseFabric(string filepath, ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream      = entry.Open();
            StreamReader sr = new(stream);
            var content     = sr.ReadToEnd();
            return FabricModParser.ParseJson(content, filepath, collection, archive);
        }

        private static ModFile ParseFML(string filepath, ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream      = entry.Open();
            StreamReader sr = new(stream);
            var content     = sr.ReadToEnd();
            return FMLModParser.ParseToml(content, filepath, collection, archive);
        }

        private static ModFile ParseFOV(string filepath, ZipArchive archive, ZipArchiveEntry entry,
            ModCollection collection)
        {
            var stream      = entry.Open();
            StreamReader sr = new(stream);
            var content     = sr.ReadToEnd();
            return FOVModParser.ParseJson(content, filepath, collection, archive);
        }

        private static ModFile ParseInv(string filepath, ModCollection collection)
        {
            Logger.Warn("PianKit.ModController", $"No parser found for {filepath}." +
                $"ModFile will be loaded as default");
            string filename = Path.GetFileName(filepath);
            if (collection.ModCache.ContainsKey(filename))
                return FabricModParser.LoadAllFromJSON(
                              collection,
                              filepath, 
                              collection.ModCache[filename].ConvertTo(Format.ToJObject, []));
            else 
                return new ModFile(filepath)
            {
                Dependencies   = [],
                ReleaseDate    = DateTime.Now,
                Version        = "unknown",
                MetaData       = new(),
                ValidStructure = false,
            };
        }
    }
}
