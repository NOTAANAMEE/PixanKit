using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;

namespace PixanKit.LaunchCore.Extension
{
    public interface ISettingModifier
    {
        public string Key { get; }

        public void ReadValue(GameBase game, JToken? token);

        public JToken WriteValue(GameBase game);
    }
}
