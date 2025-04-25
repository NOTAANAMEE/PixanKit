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
        public IGameInitor Instance { get; }

        public GameBase InitGame(string path);
    }
}
