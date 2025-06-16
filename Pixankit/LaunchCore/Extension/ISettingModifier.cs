using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;

namespace PixanKit.LaunchCore.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISettingModifier
    {
        /// <summary>
        /// Key for the setting modifier.
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="token"></param>
        public void ReadValue(GameBase game, JToken? token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public JToken WriteValue(GameBase game);
    }
}
