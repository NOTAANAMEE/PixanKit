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
    public class GamePostProcess(Folder folder, string name, string version, bool processJSON)
    {
        string name = name;

        string version = version;

        string versiondir = folder.VersionDir;

        bool processjson = processJSON;

        Folder owner = folder;

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
            JSON.SaveFromFile(name, target);

            Directory.Delete($"{versiondir}/{version}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="loaderversion"></param>
        /// <param name="name"></param>
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
