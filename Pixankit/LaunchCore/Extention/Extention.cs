using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.PlayerModule.Player;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using PixanKit.LaunchCore.SystemInf;
using PixanKit.LaunchCore.Log;
using System.Reflection.Metadata.Ecma335;

namespace PixanKit.LaunchCore.Extention
{
    public delegate GameBase? InitGame(string path);

    public delegate PlayerBase? InitPlayer(JObject? jData);

    public delegate JArray? ModListGetter(ModloaderGame game);

    public delegate long MemoryGetter();

    public static class Initors
    {
        public static InitGame GameInitor;

        public static InitPlayer PlayerInitor;

        public static MemoryGetter GetMemory;

        public static ModListGetter GetModList;

        static Initors()
        {
            GameInitor += InitG;
            PlayerInitor += InitP;
            GetMemory += GetMem;
            GetModList += GetMod;
        }

        private static GameBase? InitG(string path)
        {
            path = Localize.DeLocalize(path);
            string jsonPath = path + "/" + Path.GetFileName(path) + ".json";
            JObject jobj;
            try
            {
                jobj = JObject.Parse(File.ReadAllText(Localize.PathLocalize(jsonPath)));
            }
            catch { return null; }

            if (jobj["mainClass"].ToString() != "net.minecraft.client.main.Main") return new ModloaderGame(path, jobj);
            else return new OrdinaryGame(path, jobj);
        }

        /// <summary>
        /// {
        /// "name":"",
        /// "uid":"",
        /// "type":"",
        /// "refreshtoken":"",
        /// "accesstoken":"",
        /// 
        /// }
        /// </summary>
        /// <param name="jData"></param>
        /// <returns></returns>
        private static PlayerBase? InitP(JObject? jData) 
        {
            switch (jData["type"].ToString())
            {
                case "offline":
                    return new OfflinePlayer(jData);
                case "microsoft":
                    return new MicrosoftPlayer(jData);
                case "yggdrasil":
                default:
                    return null;
            }
        }

        private static long GetMem()
        {
            return 6000;
        }

        private static JArray? GetMod(ModloaderGame game)
        {
            if (game.Owner == null || game.Owner.Owner == null) return null;
            return (JArray?)game.Owner.Owner.GameModCache[game.GameFolder];
        }

        private static void ExtentionInit()
        {

        }

        private static void LoadExtention(string file)
        {
            Logger.Info($"Loading File {file}");
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFile(file);
            }
            catch
            {
                Logger.Error($"{file} Loading Process Failed: Not .NET Program");
                return;
            }
            Type? type = assembly.GetType("Init");
            if (type == null)
            {
                Logger.Error($"{file} Loading Process Failed: Init Class Not Found");
                return;
            }
            MethodInfo? method = type.GetMethod("InitModule");
            if (method == null)
            {
                Logger.Error($"{file} Loading Process Failed: Could Not Find Method: Init.InitModule");
                return;
            }
            if (!method.IsStatic)
            {
                Logger.Error($"{file} Loading Process Failed: Method Not Static: Init.InitModule");
                return;
            }
            method.Invoke(null, null);
            Logger.Info($"{file} Loaded Successfully");
        }

        public static void DefaultInit()
        {
            JObject jobj = new()
            {
                {"children", new JArray() },
                { "target", "" },
                { "games", new JObject() }
            };
            Files.FolderJData = jobj;
            Files.RuntimeJData = jobj;
            Files.PlayerJData = jobj;
            Files.ModJData = jobj;
        }
    }
}
