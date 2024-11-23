using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ModModule.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModModule.Module
{
    public partial class ModModule
    {
        public static ModModule? Instance { get; private set; }

        public Dictionary<string, ModInf> Mods { get; private set; } = new();

        public Dictionary<string, ModCollection> ModGames { get; private set; } = new();

        public ModModule() 
        {
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

        private void AddGame(GameBase game)
        {
            if (game.GameType == GameType.Mod) AddGame(game as ModloaderGame);
        }

        private void AddGame(ModloaderGame game)
        {
            ModCollection modcollection;
            ModGames.Add(game.Path, modcollection = new ModCollection(game, this));

            if (gameCache.ContainsKey(game.Path)) modcollection.SetCache(gameCache[game.Path]);
            else modcollection.SetCache(new JObject());
        }

        private void RemoveGame(ModloaderGame game) 
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
