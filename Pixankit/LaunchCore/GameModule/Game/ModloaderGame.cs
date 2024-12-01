using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// Minecraft Game With Mod Loader
    /// </summary>
    public class ModLoaderGame : ModifiedGame
    {
        /// <summary>
        /// The Mod path. For instance: C:\Users\Admin\AppData\.minecraft\versions\1.12.2-Forge\mods
        /// </summary>
        public string ModDir
        {
            get => _path + "/mods";
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        /// <param name="jData"><inheritdoc/></param>
        public ModLoaderGame(string path, JObject jData):base(path, jData) { _gameType = GameType.Mod; }

        public ModLoaderGame(string path) : base(path)
        {
            _gameType = GameType.Mod;
        }

        protected override void LoadJSON(JObject gameJdata)
        {
            
        }
    }
}
