using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Log;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Occurs when the launcher is closed.
        /// </summary>
        public Action? OnLauncherClosed;

        /// <summary>
        /// Closes the launcher and saves the current state of folders, Java runtimes, and players into the respective data structures.
        /// Note: The actual saving to disk must be handled separately by calling <see cref="Files.Save"/> or other appropriate methods.
        /// </summary>
        public void Close()
        {
            Logger.Info("Launcher Closing");
            Files.FolderJData = GameManager.Instance.SaveFolderData();
            Files.RuntimeJData = SaveJavaData();
            Files.PlayerJData = PlayerManager.Instance.Save();
            Logger.Info("Launcher Closed. Call Files.Save() To Save Or Handle It Yourself");
            OnLauncherClosed?.Invoke();
        }

        private JObject SaveJavaData()
        {
            JArray javaRuntimes = [];
            foreach (JavaModule.Java.JavaRuntime javaRuntime in _javaRuntimes)
            {
                javaRuntimes.Add(javaRuntime.ToJSON());
            }
            return new JObject()
            {
                { "children", javaRuntimes},
            };
        }
    }
}
