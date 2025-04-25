using PixanKit.LaunchCore.GameModule.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Extention
{
    /// <summary>
    /// Defines the interface for initializing a game instance.
    /// </summary>
    public interface IGameInitor
    {
        /// <summary>
        /// Initializes a game instance from the specified path.
        /// </summary>
        /// <param name="path">The path to the game folder. For example: <c>"C:/Users/Admin/AppData/Roaming/.minecraft/versions/1.20.1"</c>.</param>
        /// <returns>
        /// An instance of <see cref="GameBase"/> representing the initialized game.
        /// </returns>
        /// <remarks>
        /// This method is responsible for creating and configuring a game instance based on the provided path.
        /// It may involve reading configuration files, setting up libraries, and preparing runtime arguments.
        /// </remarks>
        public GameBase InitGame(string path);
    }
}
