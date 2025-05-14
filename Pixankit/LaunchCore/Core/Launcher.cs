using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core.Managers;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.PlayerModule.Player;

namespace PixanKit.LaunchCore.Core
{
    /// <summary>
    /// Represents the main launcher class for managing games, players, Java runtimes, and settings.
    /// Provides a variety of events for interaction and lifecycle management.    
    /// </summary>
    public partial class Launcher
    {
        private static readonly Lazy<Launcher> _instance = new(() => new Launcher());

        /// <summary>
        /// Gets the single instance of the <see cref="Launcher"/> class.
        /// </summary>
        public static Launcher Instance => _instance.Value;

        private Launcher()
        {
            Logger.Logger.Info("Start Initing");
            InitSettings();
            Logger.Logger.Info("Launcher Inited Successfully");
            OnLauncherInitialized?.Invoke(this);
        }

        public readonly GameManager GameManager = new();

        public readonly PlayerManager PlayerManager = new();

        public readonly JavaManager JavaManager = new();


        private void InitSettings()
        {
            if (Files.SettingsJData != null) Settings = Files.SettingsJData;
        }

        public void Init()
        {
            GameManager.InitGameModule();
            PlayerManager.InitPlayerModule();
        }

        /// <summary>
        /// Gets or sets the settings of the launcher instance.
        /// </summary>
        public JObject Settings = new()
        {
            { "java", "closest"},//"overall": the same as the overall settings, "specified": Should be the same version, "closest": The closest version(Bigger / equal), "newest": The largest version, default: user specified
            { "argument", "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true" },//default:user specified
            { "runningfolder", "self" }, //"self": self folder defult: user specified
            { "pre_argument", ""},
            { "post_argument", ""}
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
