using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModKit.ModModules;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore;
using PixanKit.LaunchCore.Extention;
using PixanKit.ModKit;
using Tomlyn;

namespace PixanKit.ModKit.ModModules
{
    /// <summary>
    /// 
    /// </summary>
    public class ModFile
    {
        /// <summary>
        /// The File Path
        /// </summary>
        public string Path
        {
            get;
            internal set;
        } = "";

        /// <summary>
        /// Mod Display Name
        /// </summary>
        public string Name
        {
            get;
            internal set;
        } = "";

        /// <summary>
        /// Mod ID
        /// </summary>
        public string ModID
        {
            get;
            internal set;
        } = "";
        
        /// <summary>
        /// Icon File Path
        /// </summary>
        public string IconFile
        {
            get;
            internal set;
        } = "";

        /// <summary>
        /// Description
        /// </summary>
        public string Description
        {
            get;
            internal set;
        } = "";

        public string Version
        {
            get;
            internal set;
        } = "";

        public string ID
        {
            get;
            internal set;
        } = "";

        public bool Valid
        {
            get;
            private set;
        } = false;

        public string[] Authors = Array.Empty<string>();

        public Dictionary<string, string> Dependencies = new();

        protected string TempConfigFile
        {
            get => Files.CacheDir + $"/Mod-{Name}-conf.txt";
        }

        private FileStream? file;

        private ZipArchive? zip;

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="filepath">FilePath Of The Mod Loader</param>
        public ModFile(string filepath)
        {
            OpenZIP();
            Valid = FabricLoad() || ForgeLoad();
            CloseZIP();
        }

        internal ModFile() { }

        #region InitorMethods
        private void LoadMod()
        {
            OpenZIP();

            CloseZIP();
        }

        private void OpenZIP()
        {
            file = new FileStream(
                Localize.PathLocalize(Path), FileMode.Open);
            zip = new ZipArchive(file);
        }
        
        private void CloseZIP()
        {
            file.Close();
            zip.Dispose();
        }

        private void Extract(string fileName)
        {
            var entry = zip.GetEntry(fileName);
            if (entry == null) throw new Exception();
            entry.ExtractToFile(Localize.PathLocalize(TempConfigFile));
        }

        private bool FabricLoad()
        {
            try { Extract("fabric.mod.json"); }
            catch { return false; }
            JObject modJData = JObject.Parse(File.ReadAllText(
                Localize.PathLocalize(TempConfigFile)));
            ID = (modJData["id"]?? new JObject()).ToString();
            Authors = (modJData["authors"] as JArray).Values<string>().ToArray() ?? Array.Empty<string>();
            Name = (modJData["name"] ?? new JObject()).ToString();
            foreach (var dependency in modJData["depends"] as JObject)
            {
                Dependencies.Add(dependency.Key, dependency.Value.ToString());
            }
            return true;
        }

        private bool ForgeLoad()
        {
            try { Extract("/META-INF/mods.toml"); }catch { return false; }
            var modTData = Toml.Parse(File.ReadAllText(
                Localize.PathLocalize(TempConfigFile)));
            var modConfig = modTData.ToModel<ForgeModConfig>();
            var mod = modConfig.Mods[0];
            ID = mod.ModId;
            Name = mod.DisplayName;
            IconFile = mod.LogoFile;
            Authors = new string[] { mod.Authors };
            Description = mod.Description;
            Version = mod.Version;
            foreach (var dependency in modConfig.Dependencies)
            {
                Dependencies.Add(dependency.ModId, dependency.VersionRange);
            }
            return true;
        }

        internal void JSONLoad(JObject modJData)
        {
            Name = modJData["name"].ToString();
            ID = modJData["modid"].ToString();
            Version = modJData["version"].ToString();
            IconFile = modJData["url"].ToString();
            Description = modJData["description"].ToString();
            Authors = (modJData["authors"] as JArray).Values<string>().ToArray();
            foreach (var depend in modJData["dependencies"] as JArray)
            {
                Dependencies.Add(depend["id"].ToString(), depend["version"].ToString());
            }
        }
        #endregion

        internal string ExtractIcon()
        {
            string iconpath = Localize.PathLocalize(
                $"{Files.CacheDir}/{ID}");
            if (IconFile == "") return "";
            if (IconFile.StartsWith("http://") || IconFile.StartsWith("https://")) return "";
            Extract(IconFile, iconpath);
            return MoveIcon();
        }

        internal string MoveIcon()
        {
            return MoveIcon(ID, IconFile[IconFile.LastIndexOf('.')..]);
        }

        internal static string MoveIcon(string id, string extension)
        {
            string iconpath = Localize.PathLocalize(
                $"{ModModule.IconCacheDir}/{id}");
            var sha1 = GetSHA1(iconpath);
            string destination = $"{ModModule.IconCacheDir}/{sha1}" +
                $"{extension}";
            try
            {
                File.Move(iconpath, Localize.PathLocalize(
                    destination));
            }
            catch { File.Delete(iconpath); }
            return destination;
        }

        private void Extract(string zippath, string filepath)
        {
            OpenZIP();
            if (zip == null) return;
            var entry = zip.GetEntry(zippath);
            if (entry == null)return;
            entry.ExtractToFile(Localize.PathLocalize(filepath));
        }

        public static string GetSHA1(string Path)
        {
            using (SHA1 sha1 = SHA1.Create())
            using (FileStream fileStream = new(Path, FileMode.Open, FileAccess.Read))
            {
                byte[] hashBytes = sha1.ComputeHash(fileStream);
                StringBuilder stringBuilder = new();
                foreach (byte b in hashBytes)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }
}
