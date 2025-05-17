using PixanKit.LaunchCore.Core.Managers;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Game;

namespace PixanKit.LaunchCore.Core;

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
        _settings = new(Files.SettingsJData ?? []);
        Logger.Logger.Info("Launcher Inited Successfully");
        OnLauncherInitialized?.Invoke(this);
    }

    public readonly GameManager GameManager = new();

    public readonly PlayerManager PlayerManager = new();

    public readonly JavaManager JavaManager = new();

    public void Init()
    {
        GameManager.InitGameModule();
        PlayerManager.InitPlayerModule();
    }

    /// <summary>
    /// Gets or sets the settings of the launcher instance.
    /// </summary>
    public GameSettings Setting => _settings;
    
    private readonly GameSettings _settings;

    /// <summary>
    /// The name of the launcher.
    /// </summary>
    public static string LauncherName = "MyLauncher";

    /// <summary>
    /// The version of the launcher.
    /// </summary>
    public static string VersionName = "1000";
}