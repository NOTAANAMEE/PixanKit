using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.Game
{
    public class OrdinaryGame: GameBase
    {
        public OrdinaryGame(string path, JObject jData): base(jData)
        {
            _path = path;
            _gameType = GameType.Ordinary;
            assetsID = jData["assetIndex"]["id"].ToString();
            javaVersion = (short)(int)jData["minimumLauncherVersion"];
        }
    }
}
