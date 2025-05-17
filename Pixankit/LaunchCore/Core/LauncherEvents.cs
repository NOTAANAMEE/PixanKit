using PixanKit.LaunchCore.Core.LaunchSession;
using PixanKit.LaunchCore.GameModule.Game;

namespace PixanKit.LaunchCore.Core;

public partial class Launcher
{
    #region LauncherEvents
    /// <summary>
    /// Occurs before launching a game. Allows modification of the launch parameters.
    /// </summary>
    /// <remarks>
    /// The first parameter is the game being launched, and the second is the Vanilla launch arguments.
    /// The method should return the modified launch arguments.
    /// </remarks>
    public static Func<GameBase, string, string>? GamePreLaunch;

    /// <summary>
    /// Occurs after a game is launched.
    /// </summary>
    public static Action<GameBase>? GamePostLaunch;

    /// <summary>
    /// Occurs when a game exits.
    /// </summary>
    /// <remarks>
    /// The first parameter is the game that exited, and the second is the process result.
    /// </remarks>
    public static Action<GameBase, ProcessResult>? GameExit;
    #endregion

    #region ThisEvents
    /// <summary>
    /// Occurs when the launcher is closed.
    /// </summary>
    public Action? OnLauncherClosed;

    /// <summary>
    /// Occurs when a new launcher instance is initialized.
    /// </summary>
    public static Action<Launcher>? OnLauncherInitialized;
    #endregion
}