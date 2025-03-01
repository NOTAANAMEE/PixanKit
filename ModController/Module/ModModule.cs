using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Log;
using PixanKit.ModController.Interfaces;
using PixanKit.ModController.Mod;
using PixanKit.ModController.ModReader;
using System.Collections.Concurrent;

namespace PixanKit.ModController.Module
{
    /// <summary>
    /// Represents the module responsible for managing mods within the launcher.
    /// </summary>
    public partial class ModModule: IToJSON
    {
        /// <summary>
        /// Initializes the ModModule class and sets up event handlers for the launcher.
        /// </summary>
        public static void Init()
        {
            //This method registers the events when game changes.
            //Launcher.LauncherInit += (a) => { _ = new ModModule(); };
            Launcher.GameAdd += (a) => { Instance?.AddJudgeGame(a); };
            Launcher.GameRemove += (a) => 
            { if (a.GetType() == typeof(ModdedGame))
                  Instance?.ModdedGames.Remove(a as ModdedGame ?? 
                  throw new Exception("Impossible exception")); };
            //Remove game. Check whether game in the dictionary.
        }

        /// <summary>
        /// Gets or sets the singleton instance of the ModModule.
        /// </summary>
        public static ModModule? Instance;

        /// <summary>
        /// Gets or sets the path to the icon cache for mods.
        /// </summary>
        public static string IconCachePath
        {
            get => Paths.GetOrAdd("ModController_IconCache", "${CacheDir}/ModIcon");
            set => Paths.TrySet("ModController_IconCache", value);
        }

        /// <summary>
        /// Gets or sets the path to the settings file for mods.
        /// </summary>
        public static string SettingsPath
        {
            get => Paths.GetOrAdd("ModController_SettingsPath", "${ConfigDir}/Modsettings.json");
            set => Paths.TrySet("ModController_SettingsPath", value);
        }

        /// <summary>
        /// A list of supported mod loaders.
        /// </summary>
        public static readonly List<string> ModLoaders = ["forge", "neoforge", "fabric", "quilt"];

        /// <summary>
        /// Gets or sets the mod version getter implementation.
        /// </summary>
        public IModVersionGetter? ModVersionGetter;

        /// <summary>
        /// A cache for storing mod-related data as a JSON object.
        /// </summary>
        private JObject ModCache = [];

        /// <summary>
        /// A dictionary containing metadata for mods.
        /// </summary>
        public ConcurrentDictionary<string, ModMetaData> ModDatas = [];

        /// <summary>
        /// A list of the tasks. Use Task.WhenAll() to wait.
        /// </summary>
        public List<Task> InitTasks = [];

        /// <summary>
        /// A dictionary containing collections of mods associated with specific modded games.
        /// </summary>
        public Dictionary<ModdedGame, ModCollection> ModdedGames = [];

        readonly object Locker = new();

        /// <summary>
        /// The locker that locks <see cref="ModDatas"/>
        /// </summary>
        public readonly object MetaDataLocker = new();

        /// <summary>
        /// Initializes a new instance of the ModModule class.
        /// Reads existing mod data and sets up collections for the launcher's games.
        /// </summary>
        public ModModule() 
        {
            //Initialize
            Instance = this;//Single-Instance
            if (Launcher.Instance == null) return;//Need to wait until Launcher inits
            lock (MetaDataLocker) ReadFile();
            foreach (var folder in Launcher.Instance.Folders)
            {
                foreach (var game in folder.Games)
                {//For each game do...
                    if (game.GameType == GameType.Modded)
                    {
                        AddJudgeGame(game);//Add game and log
                        Logger.Info("ModController", $"Modded Game Added: {game.Name}");
                    }
                }
            }
            ModCache = [];//Clear the cache to release memory
        }

        /// <summary>
        /// Reads the mod settings file and loads it into the cache.
        /// </summary>
        private void ReadFile()
        {
            var jsoncontent = JObject.Parse(File.ReadAllText(SettingsPath));
            LoadFromJSON(jsoncontent);
        }

        /// <summary>
        /// Save the default file to the file system.
        /// </summary>
        public static void DefaultFile()
        {
            var obj = new JObject()
            {
                { "games", new JObject() },
                { "metadata", new JArray(){
                    new JObject()
                    {
                        { "id", "unknown" },
                        { "name", "unknown" },
                        { "icon", "unknown" },
                        { "description", "unknown" },
                        { "author", new JArray() }
                    } }
                }
            };
            FileStream fs = new(SettingsPath, FileMode.Create);
            StreamWriter sw = new(fs);
            sw.Write(obj.ToString());
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// Reads the mod settings JObject and loads it into the cache.
        /// </summary>
        /// <param name="jsoncontent">The settings JObject</param>
        /// <exception cref="JsonException"></exception>
        public void OpenContent(JObject jsoncontent)
        {
            ModCache = jsoncontent["games"] as JObject ??
                throw new JsonException();
            foreach (var jsondata in jsoncontent["metadata"] ??
                throw new JsonException())
            {
                var metadata = FabricModParser.ParseModMetaDataFromJSON(
                    jsondata as JObject ??
                    throw new JsonException());
                AddMetaData(metadata);
            }
        }

        /// <summary>
        /// Adds metadata for a mod.
        /// </summary>
        /// <param name="data">The metadata to add.</param>
        public void AddMetaData(ModMetaData data)
        {
            if(!ModDatas.TryAdd(data.ModID, data))
                throw new Exception("Unsuccess"); 
        }

        /// <summary>
        /// Adds a collection of mods for a specific modded game.
        /// </summary>
        /// <param name="game">The modded game to associate with the mod collection.</param>
        public void AddCollection(ModdedGame game)
        {
            lock (Locker)
            if (!ModdedGames.ContainsKey(game))
                ModdedGames.Add(game, new ModCollection(
                    ModCache.GetOrDefault(Format.ToJObject, 
                    JSON.PathToKey(game.GameFolderPath), [])
                    , game));
        }

        /// <summary>
        /// Gets the collection of mods for a specific modded game.
        /// </summary>
        /// <param name="game">The modded game for which to retrieve the mod collection.</param>
        /// <returns>The mod collection associated with the game.</returns>
        public ModCollection GetCollection(ModdedGame game)
            => ModdedGames[game];

        /// <summary>
        /// Adds a game to the mod module if it is a modded game.
        /// </summary>
        /// <param name="game">The game to evaluate and add.</param>
        internal void AddJudgeGame(GameBase game)
        {
            try
            {
                if (game.GetType() == typeof(ModdedGame))
                    AddCollection(game as ModdedGame ??
                        throw new Exception("Impossible exception"));
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Warn("PixanKit.ModController", $"{game.Name} is not a modded game");
            }
        }

        /// <summary>
        /// Cleans up metadata by removing entries with a reference time of 0.
        /// </summary>
        public void CleanMetaData()
        {
            var removingId = ModDatas
                .Where(pair => pair.Value.ReferenceTime == 0)
                .Select(pair => pair.Key).ToArray();//LINQ
            foreach (var item in removingId) ModDatas.Remove(item, out _);
        }
        
        /// <summary>
        /// Save the cache to the file.
        /// </summary>
        public void SaveFile()
        {
            JObject obj = ToJSON();
            FileStream fs = new(SettingsPath, FileMode.Create);
            StreamWriter sw = new(fs);
            sw.Write(obj.ToString());
            sw.Close();
            fs.Close();
        }
    }
}
