using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.Mod;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.PlayerModule.Player;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Core
{
    /// <summary>
    /// The Launcher Class
    /// </summary>
    public partial class Launcher
    {
        /// <summary>
        /// When a new launcher instance is inited, this action will be called
        /// </summary>
        public static Action<Launcher?>? LauncherInit;

        //private static Action<Launcher?, GameBase>? GameInit;

        //public static Action<Launcher, PlayerBase>? PlayerInit;
        /// <summary>
        /// Constructs a Launcher instance. Init JObjects in Files before calling this constructor.
        /// </summary>
        public Launcher()
        {
            //if (_instance != null) throw new InvalidOperationException("Launcher is a single instance class");
            Logger.Info("Start Initing");
            InitModModule();
            InitGameModule();
            InitPlayerModule();
            InitJavaModule();
            Logger.Info("Launcher Inited Successfully");
            InitSettings();
            LauncherInit?.Invoke(this);
        }

        private void InitGameModule()
        {
            List<Folder> folders = new();
            foreach (JToken jData in Files.FolderJData["children"])
            {
                var tmp = new Folder((JObject)jData);
                tmp.SetOwner(this);
                folders.Add(tmp);
                FolderAdd?.Invoke(tmp);
            }
            _folders = folders;
            string tmpstr = Files.FolderJData["target"].ToString();
            if (tmpstr != "") TargetGame = FindGame(tmpstr);
            ResetTargetGame();
        }

        private void InitPlayerModule()
        {
            List<PlayerBase> players = new();
            foreach (JToken jData in Files.PlayerJData["children"])
            {
                PlayerBase? ret = Initors.PlayerInitor((JObject)jData) ?? throw new NullReferenceException();
                players.Add(ret);
                PlayerLoad?.Invoke(ret);
            }
            _players = players;
            string tmpstr = Files.PlayerJData["target"].ToString();
            if (tmpstr != "") TargetPlayer = FindPlayer(tmpstr);
            ResetTargetPlayer();
        }

        private void InitJavaModule()
        {
            List<JavaRuntime> javaRuntimes = new();
            foreach (JToken jData in Files.RuntimeJData["children"])
            {
                javaRuntimes.Add(new JavaRuntime((JObject)jData));
            }
            _javaRuntimes = javaRuntimes;
        }

        private void InitModModule()
        {
            Dictionary<string, ModBase> mods = new();
            foreach (JToken token in Files.ModJData["children"])
            {
                var tmp = new ModBase((JObject)token);
                mods.Add(tmp.ID, tmp);
            }
            _modCache = mods;
            JObject? jObj = (JObject?)Files.ModJData["games"];
            if (jObj != null)
                GameModCache = jObj;
        }

        private void InitSettings()
        {
            if (Files.SettingsJData != null) Settings = Files.SettingsJData;
        }

        /// <summary>
        /// The setting of the launcher instance
        /// </summary>
        public JObject Settings = new()
        {
            { "java", "closest"},//"overall": the same as the overall settings, "specified": Should be the same version, "closest": The closest version(Bigger / equal), "newest": The largest version, default: user specified
            { "argument", "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true" },//default:user specified
            { "runningfolder", "self" } //"self": self folder defult: user specified
        };

        /// <summary>
        /// The name of the launcher
        /// </summary>
        public static string LauncherName = "MyLauncher";

        /// <summary>
        /// The version of the launcher
        /// </summary>
        public static string VersionName = "1000";
    }
}
