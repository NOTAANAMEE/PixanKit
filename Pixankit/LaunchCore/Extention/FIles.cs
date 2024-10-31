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
    public static class Files
    {
        public static JObject FolderJData
        {
            get
            {
                if (_folderJData == null) throw new NullReferenceException("FolderJData not inited");
                return _folderJData;
            }
            set => _folderJData = value;
        }

        public static JObject ModJData
        {
            get
            {
                if (_modJData == null) throw new NullReferenceException();
                return _modJData;
            }
            set => _modJData = value;
        }

        public static JObject RuntimeJData
        {
            get
            {
                if (_runtimeJData == null) throw new NullReferenceException();
                return _runtimeJData;
            }
            set => _runtimeJData = value;
        }

        public static JObject PlayerJData
        {
            get
            {
                if (_playerJData == null) throw new NullReferenceException();
                return _playerJData;
            }
            set => _playerJData = value;
        }

        public static JObject? SettingsJData
        {
            get
            {
                //if (_settingsJData == null) throw new NullReferenceException();
                return _settingsJData;
            }
            set => _settingsJData = value;
        }

        public static string LauncherConfigDir = "./Launcher/Config";

        /// <summary>
        /// The native setting for every game. 
        /// For example:C:\Users\admin\AppData\Roaming\.minecraft\versions\1.20.4\settings.json
        /// </summary>
        public static string GameSettingName = "/settings.json";

        public static string ManifestSavePlace = $"{LauncherConfigDir}/manifest.json";

        public static string SkinCache = $"{LauncherConfigDir}/Cache/Skin";

        private static JObject? _folderJData = null;

        private static JObject? _modJData = null;

        private static JObject? _runtimeJData = null;

        private static JObject? _playerJData = null;

        private static JObject? _settingsJData = null;

        public static string GetSha1(string path)
        {
            FileStream fs = new(path, FileMode.Open);
            SHA1 sha1 = SHA1.Create();
            var ret = sha1.ComputeHash(fs);
            fs.Close();
            return BitConverter.ToString(ret).Replace("-", "");
        }
    }
}
