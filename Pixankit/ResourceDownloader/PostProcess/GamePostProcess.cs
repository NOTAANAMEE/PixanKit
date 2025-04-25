using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;

namespace PixanKit.ResourceDownloader.PostProcess
{
    /// <summary>
    /// This class helps to process the game after downloading
    /// </summary>
    /// <param name="folder">the folder which contains the game</param>
    /// <param name="name">the expected name of the game</param>
    /// <param name="version">the version of the game</param>
    /// <param name="processJSON">whether process JSON document or not</param>
    public class GamePostProcess(Folder folder, string name, string version, bool processJSON)
    {
        readonly string name = name;

        readonly string version = version;

        readonly string versiondir = folder.VersionDirPath;

        readonly bool processjson = processJSON;

        readonly Folder owner = folder;

        /// <summary>
        /// Start the process
        /// </summary>
        public void Process()
        {
            ProcessGame();
            if (processjson) ProcessJSON();
        }

        private void ProcessGame()
        {
            File.Copy($"{versiondir}/{version}/{version}.jar",
                $"{versiondir}/{name}/{name}.jar");
            if (owner.FindGame(version) != null) return;
            if (owner.FindVersion(version, GameType.Vanilla) != null)
            {
                Directory.Delete($"{versiondir}/{version}");
                return;
            }
            var game = Initors.GameInitor($"{versiondir}/{name}");
            if (!processjson && game is not null) GameManager.Instance.AddGame(game);
        }

        private void ProcessJSON()
        {
            var target = JSON.ReadFromFile($"{versiondir}/{version}/{version}.json");
            var merge = JSON.ReadFromFile($"{versiondir}/{name}/{name}.json");

            JSON.MergeJObject(target, merge);
            JSON.SaveFile(name, target);

            Directory.Delete($"{versiondir}/{version}");
        }

        /// <summary>
        /// Move the version folder to the new name
        /// </summary>
        /// <param name="folder">the foler which contains the game</param>
        /// <param name="loaderversion">the old name of the game</param>
        /// <param name="name">the name of the game</param>
        /// <returns>the new directory of the game</returns>
        public static string Move(Folder folder, string loaderversion, string name)
        {
            var folderpath = $"{folder.VersionDirPath}/{name}";
            var folderpath_old = $"{folder.VersionDirPath}/{loaderversion}";
            Directory.Move(folderpath_old, folderpath);
            foreach (var entry in Directory.GetFileSystemEntries(folderpath))
            {
                var filename = Path.GetFileName(entry);
                string newname;
                string newpath;
                if (!filename.StartsWith(loaderversion)) continue;
                newname = filename.Replace(loaderversion, name);
                newpath = $"{Path.GetDirectoryName(entry)}/{newname}";
                if (newname.EndsWith('/')) Directory.Move(entry, newpath);
                else File.Move(entry, newpath);
            }
            return folderpath;
        }
    }
}
