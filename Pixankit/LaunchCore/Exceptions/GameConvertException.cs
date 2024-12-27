using PixanKit.LaunchCore.GameModule.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Exceptions
{
    public class GameConvertException(GameBase game, GameType type):
        Exception($"Expects game {game.Name} to be {type}. Actual type:{game.GameType}")
    {
    }
}
