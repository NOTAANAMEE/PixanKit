using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PixanKit.ModModule.Module
{
    public partial class ModModule
    {
        static ModModule()
        {
            Path = $"{Files.ConfigDir}/ModSettings.json";
        }

        static JObject Cache = new();

        static Dictionary<string, JObject> gameCache = new();

        static Dictionary<string, JObject> ModInfCache = new();

        static string Path
        { get => Paths.Get("ModModule.ModPath"); set => Paths.TrySet("ModPath", value); }

        public static void LoadCache()
        {
            var content = File.ReadAllText(Localize.PathLocalize(Paths.Get("ModPath") + "/Mods.json"));
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
    }
}
