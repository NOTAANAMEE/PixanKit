using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.PlayerModule.Player;
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
            string jsonPath = $"{path}/{Path.GetFileName(path)}.json";
            JObject jobj = JObject.Parse(File.ReadAllText(jsonPath));


            if (jobj["mainClass"]?.ToString() != "net.minecraft.client.main.Main")
            {
                if (JudgeOptifine(jobj))
                    return new CustomizedGame(path, jobj);
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

        private static bool JudgeOptifine(JObject obj)
        {
            string mainclass = obj.GetOrDefault(Format.ToString, "mainClass", "");
            if (mainclass != "net.minecraft.launchwrapper.Launch") return false;
            string args = GameBase.GetGameArguments(obj);
            bool forge = args.Contains("fml");
            bool optifine = args.Contains("optifine");
            return !forge && optifine;
        }
    }
}
