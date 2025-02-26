using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.PlayerModule.Player;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.Extention
{
    /// <summary>
    /// Custom initialization method
    /// </summary>
    public static class Initors
    {
        /// <summary>
        /// Custom Game Initor
        /// </summary>
        public static Func<string, GameBase?> GameInitor;

        /// <summary>
        /// Custom Player Initor
        /// </summary>
        public static Func<JObject, PlayerBase?> PlayerInitor;

        /// <summary>
        /// Custom Get Memory
        /// </summary>
        public static Func<long> GetMemory;

        static Initors()
        {
            GameInitor += DefaultGameInitor;
            PlayerInitor += DefaultPlayerInitor;
            GetMemory += GetMem;
        }

        /// <summary>
        /// Default Game Initor
        /// </summary>
        /// <param name="path">The Path Of The Game</param>
        /// <returns>Game Inited</returns>
        public static GameBase DefaultGameInitor(string path)
        {
            string jsonPath = path + "/" + Path.GetFileName(path) + ".json";
            JObject jobj;
                jobj = JObject.Parse(File.ReadAllText(jsonPath));


            if (jobj["mainClass"]?.ToString() != "net.minecraft.client.main.Main")
            {
                return new ModdedGame(path, jobj);

            }
            else return new OriginalGame(path, jobj);
        }

        /// <summary>
        /// Default Player Initor
        /// </summary>
        /// <param name="jData">Example:
        /// <code>
        /// {
        /// "name":"",
        /// "uid":"",
        /// "type":"",
        /// "refreshtoken":"",
        /// "accesstoken":"",
        /// 
        /// }</code></param>
        /// <returns></returns>
        public static PlayerBase? DefaultPlayerInitor(JObject? jData) 
        {
            if (jData == null) return null;
            return (jData["type"]?.ToString()) switch
            {
                "offline" => new OfflinePlayer(jData),
                "microsoft" => new MicrosoftPlayer(jData),
                _ => null,
            };
        }

        /// <summary>
        /// Default Memory Setting
        /// </summary>
        /// <returns>6000 MB</returns>
        public static long GetMem()
        {
            long minMemory = 2048; // 2GB
            long maxMemory = 10240; // 10GB
            long availableMemory = SysInfo.GetAvailableMemSize();
            long allocatedMemory = Math.Min(maxMemory, Math.Max(minMemory, availableMemory / 2)); // 分配不超过可用内存的一半

            return allocatedMemory;
        }
    }
}
