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
    public class ModFile
    {
        #region Properties
        public string ID { get; private set; } = "";

        /// <summary>
        /// Mod Information
        /// </summary>
        public ModInf? Information { get; private set; }

        /// <summary>
        /// Path Of The Mod
        /// </summary>
        public string FilePath { get; private set; } = "";

        public string FileName { get => Path.GetFileNameWithoutExtension(FilePath); }

        /// <summary>
        /// Version Of Mod
        /// </summary>
        public Version Version { get; private set; } = new Version("0.0.0");

        public Dictionary<string, string> Dependencies { get; private set; } = new();

        public bool Release { get; private set; }

        public string SHA1 {  get; private set; }

        private bool Valid { get=> Information != null; }

        public bool IsEnabled { get => FilePath.EndsWith(".jar"); }
        #endregion

        ModCollection Owner;

        public ModFile(string filepath, ModCollection collection) 
        {
            FilePath = filepath;
            Owner = collection;
            SHA1 = Files.GetSha1(FilePath);
            LoadINF();
        }

        public void SetInf(ModInf? modInf)
        {
            if (Valid || modInf == null) return;
            SetInfEnforced(modInf);
        }

        public void SetInfEnforced(ModInf modInf)
        {
            Information = modInf;
            modInf.Ref++;
        }

        #region Dependencies
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

        public List<string> LostDependenciesR(Dictionary<string, ModFile?> mods)
        {
            List<string> list = new ();
            foreach (var dependency in Dependencies)
            {
                string depid = dependency.Key;
                if (mods.ContainsKey(depid)) {
                    if (mods[depid] != null) list.AddRange(mods[depid].LostDependenciesR(mods)); }
                else list.Add(depid);
            }
            mods[ID] = null;
            return list;
        }

        public List<string> LostDependencies(Dictionary<string, ModFile?> mods)
        {
            List<string> list = new();
            foreach (var dependency in Dependencies)
            {
                string depid = dependency.Value;
                if (!mods.ContainsKey(dependency.Value))list.Add(depid);
            }
            return list;
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
            if (json)
            { LoadDependencies(Jconf); }
            else { LoadDependencies(toml); }
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
        public void Disable()
        {
            File.Move(FilePath, FilePath += ".disabled");
        }

        public void Enable()
        {
            File.Move(FilePath, FilePath = FilePath[0..(FilePath.Length - 9)]);
            if (!IsEnabled) throw new Exception("");
        }

        #endregion

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

    public struct Version
    {
        public short Major;

        public short Minor;

        public short Patch;

        public Version(string version)
        {
            string[] parts = version.Split('.');
            Major = short.Parse(parts[0]);
            Minor = short.Parse(parts[1]);
            if (version.Length > 2) Patch = short.Parse(parts[2]);
            else Patch = 0;
        }

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

        public static bool operator == (Version lhs, Version rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator != (Version lhs, Version rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator <=(Version lhs, Version rhs)
        {
            return (lhs < rhs) || (lhs == rhs);
        }

        public static bool operator >=(Version lhs, Version rhs)
        {
            return (lhs > rhs) || (lhs == rhs);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (Patch == 0) return $"{Major}.{Minor}";
            else return $"{Major}.{Minor}.{Patch}";
        }
    }
}
