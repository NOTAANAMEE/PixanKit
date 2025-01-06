using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// Represents the Vanilla Minecraft game without Optifine or mod loaders.
    /// </summary>
    public class OriginalGame: GameBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalGame"/> class.
        /// </summary>
        /// <param name="path">The file path to the game directory.</param>
        /// <param name="jData">The JSON data containing game information.</param>
        public OriginalGame(string path, JObject jData): base(path, jData)
        {
            _gameType = GameType.Vanilla;
            assetsID = ((jData["assetIndex"] ?? new JObject())["id"]?? "").ToString();
            javaVersion = (short)(int)(jData["minimumLauncherVersion"]?? 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalGame"/> class.
        /// </summary>
        /// <param name="path">The file path to the game directory.</param>
        public OriginalGame(string path) : base(path) 
        {
            _gameType = GameType.Vanilla;
            assetsID = ((gameJSONData["assetIndex"] ?? new JObject())["id"] ?? "").ToString();
            javaVersion = (short)(int)(gameJSONData["minimumLauncherVersion"] ?? 0);
        }
    }
}
