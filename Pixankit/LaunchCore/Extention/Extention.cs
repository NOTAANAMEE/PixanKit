using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.PlayerModule.Player;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.Extention
{
    /// <summary>
    /// Provides custom initialization methods for games, players, and system settings.
    /// </summary>
    public static class Initors
    {
        /// <summary>
        /// Gets or sets the instance of the custom game initializer.
        /// </summary>
        public static IGameInitor GameInitorInstance = new DefaultGameInitor();

        /// <summary>
        /// Initializes a game instance using the specified path.
        /// </summary>
        /// <param name="path">The path to the game folder. For example: <c>"C:/Games/Minecraft"</c>.</param>
        /// <returns>An instance of <see cref="GameBase"/> representing the initialized game.</returns>
        public static GameBase GameInitor(string path)
            => GameInitorInstance.InitGame(path);

        /// <summary>
        /// A delegate for initializing a player from a JSON object.
        /// </summary>
        /// <remarks>
        /// This delegate allows customization of player initialization logic.
        /// </remarks>
        public static Func<JObject, PlayerBase?> PlayerInitor;

        /// <summary>
        /// A delegate for retrieving the system's memory allocation.
        /// </summary>
        /// <remarks>
        /// This delegate allows customization of memory allocation logic.
        /// </remarks>
        public static Func<long> GetMemory;

        /// <summary>
        /// Initializes static members of the <see cref="Initors"/> class.
        /// </summary>
        static Initors()
        {
            PlayerInitor += DefaultPlayerInitor;
            GetMemory += GetMem;
        }

        /// <summary>
        /// The default implementation for initializing a player from a JSON object.
        /// </summary>
        /// <param name="jData">A JSON object containing player data. Example:
        /// <code>
        /// {
        ///   "name": "PlayerName",
        ///   "uid": "UniqueID",
        ///   "type": "offline",
        ///   "refreshtoken": "RefreshToken",
        ///   "accesstoken": "AccessToken"
        /// }
        /// </code>
        /// </param>
        /// <returns>
        /// An instance of <see cref="PlayerBase"/> if the player type is recognized; otherwise, <c>null</c>.
        /// </returns>
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
        /// Retrieves the default memory allocation for the system.
        /// </summary>
        /// <returns>
        /// The allocated memory in megabytes. The value is determined based on the system's available memory,
        /// with a minimum of 2048 MB and a maximum of 10240 MB.
        /// </returns>
        public static long GetMem()
        {
            long minMemory = 2048; // 2GB
            long maxMemory = 10240; // 10GB
            long availableMemory = SysInfo.GetAvailableMemSize();
            long allocatedMemory = Math.Min(maxMemory, Math.Max(minMemory, availableMemory / 2));

            return allocatedMemory;
        }
    }

    /// <summary>
    /// The default implementation of the <see cref="IGameInitor"/> interface.
    /// </summary>
    internal class DefaultGameInitor : IGameInitor
    {
        /// <summary>
        /// Initializes a game instance from the specified path.
        /// </summary>
        /// <param name="path">The path to the game folder. For example: <c>"C:/Games/Minecraft"</c>.</param>
        /// <returns>
        /// An instance of <see cref="GameBase"/> representing the initialized game.
        /// </returns>
        public GameBase InitGame(string path)
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
        /// Determines whether the game is an OptiFine-modified version.
        /// </summary>
        /// <param name="obj">The JSON object containing game configuration data.</param>
        /// <returns>
        /// <c>true</c> if the game is an OptiFine-modified version; otherwise, <c>false</c>.
        /// </returns>
        private bool JudgeOptifine(JObject obj)
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
