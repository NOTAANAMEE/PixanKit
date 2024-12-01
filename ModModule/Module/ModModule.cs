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
    public partial class ModModule
    {

        /// <summary>
        /// Single Instance
        /// </summary>
        public static ModModule? Instance { get; private set; }

        /// <summary>
        /// The ModInfs List
        /// </summary>
        public Dictionary<string, ModInf> Mods { get; private set; } = new();

        /// <summary>
        /// Mod Game List
        /// </summary>
        public Dictionary<string, ModCollection> ModGames { get; private set; } = new();

        /// <summary>
        /// Initor
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public ModModule() 
        {
            if (Instance != null) 
                throw new InvalidOperationException("Single Instance Class");
            Instance = this;
            foreach (var mod in ModInfCache) 
                Mods.Add(mod.Key, ModInf.Load(mod.Value));
        }

        public void Init(Launcher launcher)
        {
            foreach (var folder in launcher.Folders) 
            {
                foreach (var game in folder.Games)AddGame(game);
            }
        }

        public async Task Init(Launcher launcher, bool parallel)
        {
            List<Task> tasks = new();
            foreach (var folder in launcher.Folders)
            {
                foreach (var game in folder.Games)
                {
                    if (game.GameType != GameType.Mod) continue;
                    var collection = AddGame(game as ModLoaderGame, false);
                    tasks.Add(Task.Run(() =>
                    {
                        if (gameCache.ContainsKey(game.Path))
                            collection.SetCache(gameCache[game.Path]);
                        else collection.SetCache(new JObject());
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
            if (gameCache.ContainsKey(game.Path)) 
                modcollection.SetCache(gameCache[game.Path]);
            else modcollection.SetCache(new JObject());
            return modcollection;
        }

        private void RemoveGame(ModLoaderGame game) 
        {
            ModGames.Remove(game.Path);
        }

        public void Close()
        {
            JObject array1 = new();
            foreach (var mod in Mods)
            {
                array1.Add(mod.Key, mod.Value.ToJSON());
            }
            JObject array2 = new();
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
