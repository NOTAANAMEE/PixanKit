using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// Minecraft Game With Mod Loader
    /// </summary>
    public class ModdedGame : CustomizedGame
    {
        /// <summary>
        /// The Mod path. For instance: C:\Users\Admin\AppData\.minecraft\versions\1.12.2-Forge\mods
        /// </summary>
        public string ModDir
        {
            get => _gameFolderPath + "/mods";
        }

        /// <summary>
        /// Stores the name of the mod loader
        /// </summary>
        public string ModLoader = "quilt";

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        /// <param name="jData"><inheritdoc/></param>
        public ModdedGame(string path, JObject jData) : base(path, jData)
        {
            _gameType = GameType.Modded;
        }

        /// <summary>
        /// Inits the game instance with specific path
        /// </summary>
        /// <param name="path">the path of the folder which contains the jar file</param>
        public ModdedGame(string path) : base(path)
        {
            _gameType = GameType.Modded;
        }

        /// <inheritdoc/>
        protected override void LoadJSON(JObject gameJdata)
        {
            base.LoadJSON(gameJdata);
            if (javaArguments.Contains("fabric")) ModLoader = "fabric";
            else if (gameArguments.Contains("neoForge")) ModLoader = "neoforge";
            else if (gameArguments.Contains("forge")) ModLoader = "forge";
        }
    }
}
