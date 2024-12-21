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

namespace ResourceDownloader.PostProcess
{
    public class GamePostProcess
    {
        string name = "";

        string version = "";

        string versiondir = "";

        bool processjson = false;

        Folder owner;

        public GamePostProcess(Folder folder, string name, string version, bool processJSON)
        {
            this.name = name;
            this.version = version;
            processjson = processJSON;
            versiondir = folder.VersionDir;
            owner = folder;
        }

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
    }
}
