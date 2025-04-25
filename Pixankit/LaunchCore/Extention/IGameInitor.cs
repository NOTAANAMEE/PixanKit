using PixanKit.LaunchCore.GameModule.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Extention
{
    public interface IGameInitor
    {
        public GameBase InitGame(string path);
    }
}
