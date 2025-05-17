using PixanKit.LaunchCore.Extension;

namespace PixanKit.LaunchCore.Core;

public partial class Launcher
{
    /// <summary>
    /// Closes the launcher and saves the current state of folders, Java runtimes, and players into the respective data structures.
    /// Note: The actual saving to disk must be handled separately by calling <see cref="Files.Save"/> or other appropriate methods.
    /// </summary>
    public void Close()
    {
        Logger.Logger.Info("Launcher Closing");
        Files.FolderJData  = GameManager.Save();
        Files.RuntimeJData = JavaManager.Save();
        Files.PlayerJData  = PlayerManager.Save();
        Logger.Logger.Info("Launcher Closed. Call Files.Save() To Save Or Handle It Yourself");
        OnLauncherClosed?.Invoke();
    }
}