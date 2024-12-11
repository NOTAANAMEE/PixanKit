using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PixanKit.ModModule.Module
{
    public partial class ModModule
    {
        /// <summary>
        /// Initializes the ModModule.
        /// Logs the initialization process and prepares the module.
        /// </summary>
        public static void Init()
        {
            _ = new ModModule();
            Launcher.LauncherInit += Instance.Init;
            Path = "${Files.ConfigDir}/ModSettings.json";
            Logger.Info("PixanKit.ModModule", "InitModule");            
        }

        static JObject Cache = [];

        static Dictionary<string, JObject> gameCache = [];

        static Dictionary<string, JObject> ModInfCache = [];

        static string Path
        { get => Paths.Get("ModModule.ModPath"); set => Paths.TrySet("ModPath", value); }

        /// <summary>
        /// Loads the cache from the mod settings file.
        /// Initializes the game cache and mod information cache.
        /// </summary>
        public static void LoadCache()
        {
            var content = File.ReadAllText(Localize.PathLocalize(Path + "/Mods.json"));
            Cache = JObject.Parse(content);
            LoadGameCache(Cache);
            LoadModInfCache(Cache);
            Cache = new();
        }

        private static void LoadGameCache(JObject jsondoc)
        {
            foreach (var item in jsondoc["games"] as JObject) 
            {
                gameCache.Add(item.Key, item.Value as JObject);
            }
        }

        private static void LoadModInfCache(JObject jsondoc)
        {
            foreach (var item in jsondoc["children"] as JObject)
            {
                gameCache.Add(item.Key, item.Value as JObject);
            }
        }

        /// <summary>
        /// Overall Init The Module. It will call the <see cref="Init()"/> method
        /// </summary>
        [ModuleInitializer]
        public static void ModuleInit() => Init();

    }
}
