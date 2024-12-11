using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Log;
using PixanKit.ModModule.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace PixanKit.ModModule.Mods
{
    /*The Mod File Class
     *How Does It Work?
     *1. Init The Class
     *2. Find From The Cache
     *3. If Cache Not Found, Load From The File
     *4. If Could Not Load From File, Tell User Invalid Mod
     *5. While The Launcher Is Closing, Close The Mod And Save The Data To Cache Too.
     */
    /// <summary>
    /// Represents a mod file and its associated data, such as dependencies and configuration.
    /// </summary>
    public class ModFile
    {
        #region Properties
        /// <summary>
        /// Gets the unique identifier of the mod.
        /// </summary>
        public string ID { get; private set; } = "";

        /// <summary>
        /// Gets the mod's metadata information.
        /// </summary>
        public ModInf? Information { get; private set; }

        /// <summary>
        /// Gets the file path of the mod.
        /// </summary>
        public string FilePath { get; private set; } = "";

        /// <summary>
        /// Gets the file name (without extension) of the mod.
        /// </summary>
        public string FileName { get => Path.GetFileNameWithoutExtension(FilePath); }

        /// <summary>
        /// Gets the version of the mod.
        /// </summary>
        public Version Version { get; private set; } = new Version("0.0.0");

        /// <summary>
        /// Gets the dependencies of the mod, where the key is the dependency ID, and the value is the version range.
        /// </summary>
        public Dictionary<string, string> Dependencies { get; private set; } = new();

        /// <summary>
        /// Gets a value indicating whether the mod is a release version.
        /// </summary>
        public bool Release { get; private set; }

        /// <summary>
        /// Gets the SHA1 checksum of the mod file.
        /// </summary>
        public string SHA1 {  get; private set; }

        /// <summary>
        /// Gets a value indicating whether the mod is enabled.
        /// </summary>
        public bool IsEnabled { get => FilePath.EndsWith(".jar"); }

        private bool Valid { get=> Information != null; }

        ModCollection Owner;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ModFile"/> class.
        /// </summary>
        /// <param name="filepath">The file path of the mod.</param>
        /// <param name="collection">The mod collection to which this mod belongs.</param>
        public ModFile(string filepath, ModCollection collection) 
        {
            FilePath = filepath;
            Owner = collection;
            SHA1 = Files.GetSha1(FilePath);
            LoadINF();
        }

        /// <summary>
        /// Sets the mod's metadata information.
        /// </summary>
        /// <param name="modInf">The metadata to set.</param>
        public void SetInf(ModInf? modInf)
        {
            if (Valid || modInf == null) return;
            SetInfEnforced(modInf);
        }

        /// <summary>
        /// Sets the mod's metadata information forcefully, bypassing validation.
        /// </summary>
        /// <param name="modInf">The metadata to set.</param>
        public void SetInfEnforced(ModInf modInf)
        {
            Owner.AddCache(modInf);
            Information = modInf;
            modInf.Ref++;
        }

        #region Dependencies
        /// <summary>
        /// Recursively retrieves lost dependencies for the mod.
        /// </summary>
        /// <param name="mods">The collection of mods to check against.</param>
        /// <returns>A list of missing dependency IDs.</returns>
        public List<string> LostDependenciesR(Dictionary<string, ModFile?> mods)
        {
            List<string> list = new();
            foreach (var dependency in Dependencies)
            {
                string depid = dependency.Key;
                if (mods.ContainsKey(depid))
                {
                    if (mods[depid] != null) list.AddRange(mods[depid].LostDependenciesR(mods));
                }
                else list.Add(depid);
            }
            mods[ID] = null;
            return list;
        }

        /// <summary>
        /// Retrieves direct lost dependencies for the mod.
        /// </summary>
        /// <param name="mods">The collection of mods to check against.</param>
        /// <returns>A list of missing dependency IDs.</returns>
        public List<string> LostDependencies(Dictionary<string, ModFile?> mods)
        {
            List<string> list = new();
            foreach (var dependency in Dependencies)
            {
                string depid = dependency.Value;
                if (!mods.ContainsKey(dependency.Value)) list.Add(depid);
            }
            return list;
        }

        internal void LoadDependencies(JObject dependJdata)
        {
            foreach (var item in dependJdata)
            {
                Dependencies.Add(item.Key, (item.Value ?? "").ToString());
            }
        }

        internal void LoadDependencies(TomlTable tomldoc)
        {
            var array = (tomldoc["dependencies"] as TomlTable).First().Value as TomlTableArray;
            foreach (TomlTable item in array)
            {
                if (item == null) continue;
                if (item.ContainsKey("mandatory") && (bool)item["mandatory"] == false) continue;
                Dependencies.Add(item["modId"] as string, item["versionRange"] as string);
            }
        }
        #endregion

        #region LOADINF
        private void LoadINF()
        {
            JObject Jconf = new(); TomlTable toml = new();
            //Try Load From Cache
            if (Owner.LoadFromSHA1(this)) return;
            //Open Config
            var zip = OpenZIP();
            ConfigFile config = new("", 0);
            try
            {
                config = ExtractConfigFile(zip);
            }
            catch(FileNotFoundException)
            {
                ID = FileName;
                return;
            }
            CloseZIP(zip);
            //Try Load From IDCache
            if (config.Config == ConfigType.json) Jconf = LoadIDJ(config);
            else toml = LoadIDT(config);
            RemoveConfigFile(config);
            if (Owner.LoadFromID(this)) return;
            //Directly Load From Config
            if (Owner.TryLoadInfFromOwnerCache(this)) 
                ValidLoadConf(Jconf, toml, config.Config == ConfigType.json);
            else InvalidLoadConf(Jconf, toml, config.Config == ConfigType.json);
            Logger.Info("ModModule", "null");
        }

        private void ValidLoadConf(JObject Jconf, TomlTable toml, bool json)
        {
            if (json) LoadDependencies(Jconf);
            else LoadDependencies(toml);
        }

        private void InvalidLoadConf(JObject Jconf, TomlTable toml, bool json)
        {
            ModInf inf;
            if (json)
            { inf = ModInf.Load(Jconf); LoadDependencies(Jconf["depends"] as JObject); Version = new(Jconf["version"].ToString()); }
            else { inf = ModInf.Load(toml); LoadDependencies(toml); }
            Information = inf;
        }

        private JObject LoadIDJ(ConfigFile config)
        {
            JObject jobj = JObject.Parse(File.ReadAllText(
                Localize.PathLocalize(config.FilePath)));

            ID = (jobj["id"] ?? "").ToString();
            return jobj;
        }

        private TomlTable LoadIDT(ConfigFile config)
        {
            var table = Toml.Parse(File.ReadAllText(config.FilePath)).ToModel();

            ID = (table["mods"] as TomlTableArray)[0]["modId"] as string;
            return table;
        }

        #endregion

        #region Zip
        private enum ConfigType
        {
            tmol,
            json
        }

        private record ZipArchiveInf(ZipArchive Archive, FileStream FileStream);
        
        private record ConfigFile(string FilePath, ConfigType Config);

        private ZipArchiveInf OpenZIP()
        {
            FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            ZipArchive archive = new(fs);
            return new(archive, fs);
        }

        private void CloseZIP(ZipArchiveInf zip) 
        {
            zip.Archive.Dispose();
            zip.FileStream.Close();
        }

        private ConfigFile ExtractConfigFile(ZipArchiveInf zip)
        {
            ZipArchiveEntry? entry;
            string name;
            bool type = (entry = zip.Archive.GetEntry(name = "fabric.mod.json")) != null;
            if (!type) entry = zip.Archive.GetEntry(name = "META-INF/mods.toml");

            if (entry == null) throw new FileNotFoundException("FileNotFound");

            string configpath = Localize.PathLocalize($"{Files.CacheDir}/mods/{SHA1}-conf");
            Directory.CreateDirectory(Path.GetDirectoryName(configpath));
            if (!File.Exists(configpath))
            entry.ExtractToFile(configpath);

            return new(configpath, type? ConfigType.json : ConfigType.tmol);
        }

        private void RemoveConfigFile(ConfigFile file)
        {
            File.Delete(file.FilePath);
        }
        #endregion

        #region Disable
        /// <summary>
        /// Disables the mod by removing its associated file.
        /// </summary>
        public void Disable()
        {
            File.Move(FilePath, FilePath += ".disabled");
        }

        /// <summary>
        /// Enables the mod by ensuring its associated file is active.
        /// </summary>
        public void Enable()
        {
            File.Move(FilePath, FilePath = FilePath[0..(FilePath.Length - 9)]);
            if (!IsEnabled) throw new Exception("");
        }
        #endregion

        /// <summary>
        /// Converts the mod information to a JSON object.
        /// </summary>
        /// <returns>A JSON representation of the mod.</returns>
        public JObject ToJSON()
        {
            JObject obj = new();
            foreach (var item in Dependencies)
            {
                obj.Add(item.Key, item.Value);
            }
            return new()
            {
                { "id",ID },
                { "version", Version.ToString() },
                { "dependencies", obj }
            };
        }
    }

    /// <summary>
    /// Represents a semantic version with major, minor, and patch components.
    /// </summary>
    public struct Version
    {
        /// <summary>
        /// The major version number.
        /// </summary>
        public short Major;

        /// <summary>
        /// The minor version number.
        /// </summary>
        public short Minor;

        /// <summary>
        /// The patch version number.
        /// </summary>
        public short Patch;

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> struct from a version string.
        /// </summary>
        /// <param name="version">The version string in the format "Major.Minor[.Patch]".</param>
        /// <exception cref="FormatException">Thrown if the version string is not in the correct format.</exception>
        public Version(string version)
        {
            string[] parts = version.Split('.');
            Major = short.Parse(parts[0]);
            Minor = short.Parse(parts[1]);
            if (version.Length > 2) Patch = short.Parse(parts[2]);
            else Patch = 0;
        }

        /// <summary>
        /// Compares two versions to determine if the left-hand version is greater than the right-hand version.
        /// </summary>
        public static bool operator >(Version lhs, Version rhs)
        {
            if (lhs.Major > rhs.Major) return true;
            else if (lhs.Major < rhs.Major) return false;
            else
            {
                if (lhs.Minor > rhs.Minor) return true;
                else if (lhs.Minor < rhs.Minor) return false;
                else return lhs.Patch > rhs.Patch;
            }
        }

        /// <summary>
        /// Compares two versions to determine if the left-hand version is less than the right-hand version.
        /// </summary>
        public static bool operator <(Version lhs, Version rhs)
        {
            if (lhs.Major < rhs.Major) return true;
            else if (lhs.Major > rhs.Major) return false;
            else
            {
                if (lhs.Minor < rhs.Minor) return true;
                else if (lhs.Minor > rhs.Minor) return false;
                else return lhs.Patch < rhs.Patch;
            }
        }

        /// <summary>
        /// Determines whether two versions are equal.
        /// </summary>
        public static bool operator ==(Version lhs, Version rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Determines whether two versions are not equal.
        /// </summary>
        public static bool operator !=(Version lhs, Version rhs) => !(lhs == rhs);

        /// <summary>
        /// Compares two versions to determine if the left-hand version is less than or equal to the right-hand version.
        /// </summary>
        public static bool operator <=(Version lhs, Version rhs) => (lhs < rhs) || (lhs == rhs);

        /// <summary>
        /// Compares two versions to determine if the left-hand version is greater than or equal to the right-hand version.
        /// </summary>
        public static bool operator >=(Version lhs, Version rhs) => (lhs > rhs) || (lhs == rhs);

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }

        /// <summary>
        /// Returns a string representation of the version.
        /// </summary>
        /// <returns>The version string in the format "Major.Minor.Patch" or "Major.Minor" if the patch is 0.</returns>
        public override string ToString()
        {
            return Patch == 0 ? $"{Major}.{Minor}" : $"{Major}.{Minor}.{Patch}";
        }
    }
}
