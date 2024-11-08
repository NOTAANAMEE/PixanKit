using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Extention
{
    /// <summary>
    /// Some Settings About Files
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Folder JSON Data  
        /// </summary>
        public static JObject FolderJData
        {
            get
            {
                if (_folderJData == null) throw new NullReferenceException("FolderJData not inited");
                return _folderJData;
            }
            set => _folderJData = value;
        }

        /// <summary>
        /// Mod JSON Data
        /// </summary>
        public static JObject ModJData
        {
            get
            {
                if (_modJData == null) throw new NullReferenceException();
                return _modJData;
            }
            set => _modJData = value;
        }

        /// <summary>
        /// Runtime JSON Data
        /// </summary>
        public static JObject RuntimeJData
        {
            get
            {
                if (_runtimeJData == null) throw new NullReferenceException();
                return _runtimeJData;
            }
            set => _runtimeJData = value;
        }

        /// <summary>
        /// Player JSON Data
        /// </summary>
        public static JObject PlayerJData
        {
            get
            {
                if (_playerJData == null) throw new NullReferenceException();
                return _playerJData;
            }
            set => _playerJData = value;
        }

        /// <summary>
        /// Setting JSON Data
        /// </summary>
        public static JObject? SettingsJData
        {
            get
            {
                //if (_settingsJData == null) throw new NullReferenceException();
                return _settingsJData;
            }
            set => _settingsJData = value;
        }

        /// <summary>
        /// Directory For Launcher Configuration Files.
        /// </summary>
        public static string LauncherConfigDir = "./Launcher/Config";

        /// <summary>
        /// The native setting for every game. 
        /// For example:C:\Users\admin\AppData\Roaming\.minecraft\versions\1.20.4\settings.json
        /// </summary>
        public static string GameSettingName = "/Launcher/settings.json";

        /// <summary>
        /// Dir For Minecraft Version Manifest
        /// </summary>
        public static string ManifestSavePlace = $"{LauncherConfigDir}/Cache/manifest.json";

        /// <summary>
        /// Dir For Skin Cache
        /// </summary>
        public static string SkinCache = $"{LauncherConfigDir}/Cache/Skin";

        private static JObject? _folderJData = null;

        private static JObject? _modJData = null;

        private static JObject? _runtimeJData = null;

        private static JObject? _playerJData = null;

        private static JObject? _settingsJData = null;

        /// <summary>
        /// Get SHA1 From File
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns>SHA1 String</returns>
        public static string GetSha1(string path)
        {
            FileStream fs = new(path, FileMode.Open);
            SHA1 sha1 = SHA1.Create();
            var ret = sha1.ComputeHash(fs);
            fs.Close();
            return BitConverter.ToString(ret).Replace("-", "");
        }

        /// <summary>
        /// Generate Default JSON Data
        /// </summary>
        public static void Generate()
        {
            ModJData = (JObject)DefaultJSON.JData.DeepClone();
            FolderJData = (JObject)DefaultJSON.JData.DeepClone();
            FolderJData.Remove("games");
            PlayerJData = (JObject)FolderJData.DeepClone();
            RuntimeJData = (JObject)FolderJData.DeepClone();
            RuntimeJData.Remove("target");
            SettingsJData = (JObject)DefaultJSON.SettingJData.DeepClone();
        }

        /// <summary>
        /// Save JSON To Default Path
        /// </summary>
        public static void Save()
        {
            FileStream modFS = new ($"{LauncherConfigDir}/ModInf.json", FileMode.Create),
                       folderFS = new($"{LauncherConfigDir}/Folders.json", FileMode.Create),
                       playerFS = new($"{LauncherConfigDir}/Players.json", FileMode.Create),
                       runtimeFS = new($"{LauncherConfigDir}/JavaRuntime.json", FileMode.Create),
                       settingsFS = new($"{LauncherConfigDir}/Settings.json", FileMode.Create);
            StreamWriter modsw = new(modFS),
                         foldersw = new(folderFS),
                         playersw = new(playerFS),
                         runtimesw = new(runtimeFS),
                         settingsw = new(settingsFS);
            modsw.Write(ModJData.ToString());
            foldersw.Write(FolderJData.ToString());
            playersw.Write(PlayerJData.ToString());
            runtimesw.Write(RuntimeJData.ToString());
            settingsw.Write(SettingsJData.ToString());
            modsw.Close();
            foldersw.Close();
            playersw.Close();
            runtimesw.Close();
            settingsw.Close();
            modFS.Close();
            folderFS.Close();
            playerFS.Close();
            runtimeFS.Close();
            settingsFS.Close();
        }

        /// <summary>
        /// Load Data From Default Path
        /// </summary>
        public static void Load() 
        {
            FileStream modFS = new($"{LauncherConfigDir}/ModInf.json", FileMode.Open),
                       folderFS = new($"{LauncherConfigDir}/Folders.json", FileMode.Open),
                       playerFS = new($"{LauncherConfigDir}/Players.json", FileMode.Open),
                       runtimeFS = new($"{LauncherConfigDir}/JavaRuntime.json", FileMode.Open),
                       settingsFS = new($"{LauncherConfigDir}/Settings.json", FileMode.Open);
            StreamReader modsr = new(modFS),
                         foldersr = new(folderFS),
                         playersr = new(playerFS),
                         runtimesr = new(runtimeFS),
                         settingsr = new(settingsFS);
            Task<string> t0 = modsr.ReadToEndAsync(),
                         t1 = foldersr.ReadToEndAsync(),
                         t2 = playersr.ReadToEndAsync(),
                         t3 = runtimesr.ReadToEndAsync(),
                         t4 = settingsr.ReadToEndAsync();
            Task.WaitAll(t0, t1, t2, t3, t4);
            _modJData = JObject.Parse(t0.Result);
            _folderJData = JObject.Parse(t1.Result);
            _playerJData = JObject.Parse(t2.Result);
            _runtimeJData = JObject.Parse(t3.Result);
            _settingsJData = JObject.Parse(t4.Result);
            modFS.Close();
            folderFS.Close();
            playerFS.Close();
            runtimeFS.Close();
            settingsFS.Close();
        }
    }

    internal static class DefaultJSON
    {
        public static JObject JData = new()
        {
            { "children", new JArray() },
            { "games", new JObject() },
            { "target", "" }
        };

        public static JObject SettingJData = new()
        {
            { "java", "closest" },
            { "arguments", "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true" },
            { "runningfolder", "self" }
        };
    }
}
