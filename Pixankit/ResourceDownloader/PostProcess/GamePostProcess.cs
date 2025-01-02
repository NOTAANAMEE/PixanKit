using PixanKit.LaunchCore.GameModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.Game;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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

        readonly string versiondir = folder.VersionDir;

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
            if (owner.FindVersion(version, GameType.Original) != null)
            {
                Directory.Delete($"{versiondir}/{version}");
                return;
            }
            if (!processjson) owner.AddGame(new OriginalGame($"{versiondir}/{name}"));
        }

        private void ProcessJSON()
        {
            JObject target = JSON.ReadFromFile($"{versiondir}/{version}/{version}.json");
            JObject merge = JSON.ReadFromFile($"{versiondir}/{name}/{name}.json");

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
            string folderpath = $"{folder.VersionDir}/{name}";
            string folderpath_old = $"{folder.VersionDir}/{loaderversion}";
            Directory.Move(folderpath_old, folderpath);
            foreach (var entry in Directory.GetFileSystemEntries(folderpath))
            {
                string filename = Path.GetFileName(entry);
                string newname = "";
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
