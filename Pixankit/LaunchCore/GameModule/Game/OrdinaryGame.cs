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
    /// Ordinary Game. Minecraft Game Without Optifine Or Mod Loaders
    /// </summary>
    public class OrdinaryGame: GameBase
    {
        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        /// <param name="jData"><inheritdoc/></param>
        public OrdinaryGame(string path, JObject jData): base(path, jData)
        {
            _gameType = GameType.Ordinary;
            assetsID = ((jData["assetIndex"] ?? new JObject())["id"]?? "").ToString();
            javaVersion = (short)(int)(jData["minimumLauncherVersion"]?? 0);
        }
    }
}
