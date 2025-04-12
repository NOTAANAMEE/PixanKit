using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Log;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Closes the launcher and saves the current state of folders, Java runtimes, and players into the respective data structures.
        /// Note: The actual saving to disk must be handled separately by calling <see cref="Files.Save"/> or other appropriate methods.
        /// </summary>
        public void Close()
        {
            Logger.Info("Launcher Closing");
            Files.FolderJData = SaveFolderData();
            Files.RuntimeJData = SaveJavaData();
            Files.PlayerJData = SavePlayerData();
            Logger.Info("Launcher Closed. Call Files.Save() To Save Or Handle It Yourself");
        }

        private JObject SaveFolderData()
        {
            Logger.Info("Game Module Closing");
            JArray folders = [];
            foreach (var folder in _folders)
            {
                folders.Add(folder.ToJSON());
                folder.Close();
            }
            return new JObject()
            {
                { "children", folders},
                { "target", TargetGame?.GameJarFilePath?? "" }
            };
        }

        private JObject SaveJavaData()
        {
            JArray javaRuntimes = [];
            foreach (var javaRuntime in _javaRuntimes)
            {
                javaRuntimes.Add(javaRuntime.ToJSON());
            }
            return new JObject()
            {
                { "children", javaRuntimes},
            };
        }

        private JObject SavePlayerData()
        {
            JArray players = new();
            foreach (var player in _players)
            {
                players.Add(player.ToJSON());
            }
            return new JObject()
            {
                { "children", players},
                { "target", TargetPlayer?.UID ?? "" }
            };
        }
    }
}
