using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Log;
using PixanKit.ModModule.Mods;
using PixanKit.ModModule.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModModule.Module
{
    /// <summary>
    /// Represents the module responsible for managing mods and their collections.
    /// </summary>
    public partial class ModModule
    {
        /// <summary>
        /// Gets the single instance of the <see cref="ModModule"/> class.
        /// </summary>
        public static ModModule? Instance { get; private set; }

        /// <summary>
        /// Gets the dictionary of mod information, where the key is the mod ID and the value is the <see cref="ModInf"/> object.
        /// </summary>
        public Dictionary<string, ModInf> Mods { get; private set; } = [];

        /// <summary>
        /// Gets the dictionary of mod collections, where the key is the game ID and the value is the <see cref="ModCollection"/> object.
        /// </summary>
        public Dictionary<string, ModCollection> ModGames { get; private set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ModModule"/> class.
        /// Ensures that only one instance of the class can exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if an instance of the class already exists.</exception>
        public ModModule() 
        {
            if (Instance != null) 
                throw new InvalidOperationException("Single Instance Class");
            Instance = this;
            foreach (var mod in ModInfCache) 
                Mods.Add(mod.Key, ModInf.Load(mod.Value));
        }

        /// <summary>
        /// Initializes the mod module with a specified launcher.
        /// This method processes all games in the launcher's folders and adds them to the module.
        /// </summary>
        /// <param name="launcher">The launcher containing the folders and games to initialize.</param>
        public void Init(Launcher launcher)
        {
            foreach (var folder in launcher.Folders) 
            {
                foreach (var game in folder.Games)AddGame(game);
            }
        }

        /// <summary>
        /// Initializes the mod module with a specified launcher using asynchronous tasks.
        /// Allows parallel initialization if specified.
        /// </summary>
        /// <param name="launcher">The launcher containing the folders and games to initialize.</param>
        /// <param name="parallel">Determines whether to initialize the games in parallel.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        public async Task Init(Launcher launcher, bool parallel)
        {
            List<Task> tasks = [];
            foreach (var folder in launcher.Folders)
            {
                foreach (var game in folder.Games)
                {
                    if (game.GameType != GameType.Mod) continue;
                    var collection = AddGame(game as ModLoaderGame, false);
                    tasks.Add(Task.Run(() =>
                    {
                        JObject cache = [];
                        if (gameCache.TryGetValue(game.Path, out cache))
                            collection.SetCache(cache);
                        else collection.SetCache([]);
                    }));
                }
            }
            await Task.WhenAll(tasks);
        }

        private void AddGame(GameBase game)
        {
            if (game.GameType == GameType.Mod) AddGame(game as ModLoaderGame);
        }

        private void AddGame(ModLoaderGame game)
        {
            AddGame(game, true);
        }

        private ModCollection AddGame(ModLoaderGame game, bool init)
        {
            Logger.Info("PixanKit.ModModule", "Start Initing Games");
            ModCollection modcollection;
            ModGames.Add(game.Path, modcollection = new ModCollection(game, this));
            if (!init) return modcollection;
            if (gameCache.TryGetValue(game.Path, out JObject cache)) 
                modcollection.SetCache(cache);
            else modcollection.SetCache([]);
            return modcollection;
        }

        private void RemoveGame(ModLoaderGame game) 
        {
            ModGames.Remove(game.Path);
        }

        /// <summary>
        /// Close the mod module
        /// </summary>
        public void Close()
        {
            JObject array1 = [];
            foreach (var mod in Mods)
            {
                array1.Add(mod.Key, mod.Value.ToJSON());
            }
            JObject array2 = [];
            foreach(var game in ModGames)
            {
                array2.Add(game.Key, game.Value.ToJSON());
            }
            Cache = new()
            {
                { "children", array1 },
                { "games", array2 } 
            };
        }
    }
}
