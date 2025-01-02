using PixanKit.LaunchCore.GameModule.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Exceptions
{
    /// <summary>
    /// It is an exception that will not be thrown.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="type"></param>
    public class GameConvertException(GameBase game, GameType type):
        Exception($"Expects game {game.Name} to be {type}. Actual type:{game.GameType}")
    {
    }
}
