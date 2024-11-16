using Newtonsoft.Json.Linq;
using Pixanit.ModKit.ModModules;
using PixanKit.LaunchCore;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ModKit.ModModules;
using System.Collections;

namespace PixanKit.ModKit
{
    public class ModModule:IEnumerable<ModCollection>
    {
        public static ModModule? Instance { get; private set; } = null;

        public Dictionary<ModloaderGame, ModCollection> ModGames;

        /// <summary>
        /// Mod Cache Is For Mods That Cannot Be Read
        /// </summary>
        public static JObject ModCache = new()
        {
            { "mods", new JArray() },
            { "icon", new JObject() },
        };

        public static string IconCacheDir { get => $"{Files.CacheDir}/icons"; }

        static ModModule()
        {
            Launcher.LauncherInit += (a) => { new ModModule(); };
        }

        public ModModule()
        {
            if (Instance != null) throw new Exception("Invalid Operation");
            Instance = this;
            foreach (Folder f in Launcher.Instance.Folders)
                foreach (GameBase game in f.Games)
                {
                    if (game.GameType == GameType.Mod) AddGame(game as ModloaderGame);
                }
            Launcher.GameAdd += (a) => {
                if (a.GameType == GameType.Mod)
                    Instance.AddGame(a as ModloaderGame);
            };
            Launcher.GameRemove += (a) => {
                if (a.GameType == GameType.Mod)
                    Instance.RemoveGame(a as ModloaderGame);
            };
        }

        public void AddGame(ModloaderGame game)
        {
            ModGames.Add(game, new ModCollection(game));
        }

        public void RemoveGame(ModloaderGame game)
        {
            ModGames.Remove(game);
        }

        /// <summary>
        /// {
        ///     "modid":"",
        ///     "name":"",
        ///     "version":"",
        ///     "iconurl":"",
        ///     "description":"",
        ///     "authors":[],
        ///     "dependencies":[{"modid":"", "version":""}]
        /// }
        /// </summary>
        /// <param name="jobj"></param>
        public void AddModCache(string sha1, JObject modJCache)
        {
            (ModCache["mods"] as JObject).Add(sha1, modJCache);
            var exists = (ModCache["icon"] as JObject).ContainsKey(modJCache["id"].ToString());
            if (exists) return;
            _ = UpdateImage(modJCache);
        }

        public async Task UpdateImage(JObject modJCache)
        {
            string url = modJCache["iconurl"].ToString();
            await DownloadImage(modJCache);

            (ModCache["icon"] as JObject).Add(modJCache["id"].ToString(),
                ModFile.MoveIcon(modJCache["id"].ToString(), url[url.LastIndexOf('.')..]));
        }

        private async Task DownloadImage(JObject modJCache)
        {
            string url = modJCache["iconurl"].ToString();
            string id = modJCache["modid"].ToString();
            string iconpath = $"{IconCacheDir}/{id}{url[url.LastIndexOf('.')..]}";
            HttpClient client = new();
            var response = await client.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            FileStream fs = new(Localize.PathLocalize(iconpath), FileMode.Create);
            stream.CopyTo(fs);
            stream.Dispose();
            fs.Close();
            client.Dispose();
        }

        public IEnumerator<ModCollection> GetEnumerator()
        {
            return ModGames.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ModGames.Values.GetEnumerator();
        }
    }
}
