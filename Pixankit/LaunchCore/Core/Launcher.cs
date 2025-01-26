using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
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
    /// Represents the main launcher class for managing games, players, Java runtimes, and settings.
    /// Provides a variety of events for interaction and lifecycle management.    
    /// </summary>
    public partial class Launcher
    {
        /// <summary>
        /// Gets the single instance of the <see cref="Launcher"/> class.
        /// </summary>
        public static Launcher? Instance = null;

        /// <summary>
        /// Occurs when a new launcher instance is initialized.
        /// </summary>
        public static Action<Launcher>? LauncherInit;

        /// <summary>
        /// Initializes a new instance of the <see cref="Launcher"/> class.
        /// This constructor initializes game, player, and Java modules, and loads settings.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if an instance of the launcher already exists.</exception>        
        public Launcher()
        {
            if (Instance != null) throw new InvalidOperationException("Launcher is a single instance class");
            Instance = this;
            Logger.Info("Start Initing");
            InitGameModule();
            InitPlayerModule();
            InitJavaModule();
            Logger.Info("Launcher Inited Successfully");
            InitSettings();
            LauncherInit?.Invoke(this);
        }

        private void InitGameModule()
        {
            List<Folder> folders = [];
            foreach (JToken jData in Files.FolderJData["children"] ?? new JObject())
            {
                var tmp = new Folder((JObject)jData);
                tmp.SetOwner(this);
                folders.Add(tmp);
                FolderAdd?.Invoke(tmp);
            }
            _folders = folders;
            string tmpstr = (Files.FolderJData["target"]?? "").ToString();
            if (tmpstr != "") TargetGame = FindGame(tmpstr);
            UpdateTargetGame();
        }

        private void InitPlayerModule()
        {
            List<PlayerBase> players = [];
            foreach (JToken jData in Files.PlayerJData["children"] ?? new JArray())
            {
                PlayerBase ret = Initors.PlayerInitor((JObject)jData) ?? throw new NullReferenceException();
                players.Add(ret);
                PlayerLoad?.Invoke(ret);
            }
            _players = players;
            string targetID = Files.PlayerJData["target"]?.ToString() ?? "";
            if (targetID != "") TargetPlayer = FindPlayer(targetID);
            ResetTargetPlayer();
        }

        private void InitJavaModule()
        {
            List<JavaRuntime> javaRuntimes = new();
            foreach (JToken jData in Files.RuntimeJData["children"] ?? new JObject())
            {
                javaRuntimes.Add(new JavaRuntime((JObject)jData));
            }
            _javaRuntimes = javaRuntimes;
        }

        private void InitSettings()
        {
            if (Files.SettingsJData != null) Settings = Files.SettingsJData;
        }

        /// <summary>
        /// Gets or sets the settings of the launcher instance.
        /// </summary>
        public JObject Settings = new()
        {
            { "java", "closest"},//"overall": the same as the overall settings, "specified": Should be the same version, "closest": The closest version(Bigger / equal), "newest": The largest version, default: user specified
            { "argument", "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true" },//default:user specified
            { "runningfolder", "self" } //"self": self folder defult: user specified
        };

        /// <summary>
        /// The name of the launcher.
        /// </summary>
        public static string LauncherName = "MyLauncher";

        /// <summary>
        /// The version of the launcher.
        /// </summary>
        public static string VersionName = "1000";
    }
}
