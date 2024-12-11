using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.PlayerModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        #region GameEvents
        /// <summary>
        /// Occurs when a game is loaded from a file.
        /// </summary>
        public static Action<GameBase>? GameLoad;

        /// <summary>
        /// Occurs when the default game is changed.
        /// </summary>
        public static Action<GameBase?>? TargetGameChange;

        /// <summary>
        /// Occurs when a new game is added.
        /// </summary>
        public static Action<GameBase>? GameAdd;

        /// <summary>
        /// Occurs when a game is removed.
        /// </summary>
        public static Action<GameBase>? GameRemove;

        /// <summary>
        /// Occurs when a new folder is added.
        /// </summary>
        public static Action<Folder>? FolderAdd;

        /// <summary>
        /// Occurs when a folder is removed.
        /// </summary>
        public static Action<Folder>? FolderRemove;

        /// <summary>
        /// Occurs before launching a game. Allows modification of the launch parameters.
        /// </summary>
        /// <remarks>
        /// The first parameter is the game being launched, and the second is the original launch arguments.
        /// The method should return the modified launch arguments.
        /// </remarks>
        public static Func<GameBase, string, string>? GamePreLaunch;

        /// <summary>
        /// Occurs after a game is launched.
        /// </summary>
        public static Action<GameBase>? GamePostLaunch;

        /// <summary>
        /// Occurs when a game exits.
        /// </summary>
        /// <remarks>
        /// The first parameter is the game that exited, and the second is the process result.
        /// </remarks>
        public static Action<GameBase, ProcessResult>? GameExit;
        #endregion

        #region PlayerEvents
        /// <summary>
        /// Occurs when a player profile is loaded from a JSON file.
        /// </summary>
        public static Action<PlayerBase>? PlayerLoad;

        /// <summary>
        /// Occurs when a new player is added.
        /// </summary>
        public static Action<PlayerBase>? PlayerAdd;

        /// <summary>
        /// Occurs when a player is removed.
        /// </summary>
        public static Action<PlayerBase>? PlayerRemove;

        /// <summary>
        /// Occurs when a player's profile is changed.
        /// </summary>
        public static Action<PlayerBase>? ProfileChange;
        #endregion
    }
}
