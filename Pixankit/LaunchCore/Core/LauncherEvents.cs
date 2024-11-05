using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.Mod;
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
        /// Game Load From File Event
        /// </summary>
        public static Action<GameBase>? GameLoad;

        /// <summary>
        /// Default Game Change Event
        /// </summary>
        public static Action<GameBase>? DefaultGameChange;

        /// <summary>
        /// Game Add Event
        /// </summary>
        public static Action<GameBase>? GameAdd;

        /// <summary>
        /// Folder Remove Event
        /// </summary>
        public static Action<GameBase>? GameRemove;

        /// <summary>
        /// Folder Add Event
        /// </summary>
        public static Action<Folder>? FolderAdd;

        /// <summary>
        /// Folder Remove Event
        /// </summary>
        public static Action<Folder>? FolderRemove;

        /// <summary>
        /// Pre-launch Event
        /// </summary>
        public static Func<GameBase, string, string>? GamePreLaunch;

        /// <summary>
        /// Post-launch Event
        /// </summary>
        public static Func<GameBase>? GamePostLaunch;

        /// <summary>
        /// Game Exit Event
        /// </summary>
        public static Action<GameBase, ProcessResult>? GameExit;

        /// <summary>
        /// Mod Information Load
        /// </summary>
        public static Action<ModBase>? ModBaseLoad;

        /// <summary>
        /// Mod Load To A Game
        /// </summary>
        public static Action<ModFile, ModloaderGame>? ModLoad;

        /// <summary>
        /// Mod Information Add Event
        /// </summary>
        public static Action<ModBase>? ModBaseAdd;

        /// <summary>
        /// Mod Information Remove Event
        /// </summary>
        public static Action<ModBase>? ModBaseRemove;

        /// <summary>
        /// Mod File Add Event
        /// </summary>
        public static Action<ModFile, ModloaderGame>? ModAdd;

        /// <summary>
        /// Mod File Remove Event
        /// </summary>
        public static Action<ModFile, ModloaderGame>? ModRemove;
        #endregion

        #region PlayerEvents
        /// <summary>
        /// Player Load From JSON Event
        /// </summary>
        public static Action<PlayerBase>? PlayerLoad;

        /// <summary>
        /// Player Add Event
        /// </summary>
        public static Action<PlayerBase>? PlayerAdd;

        /// <summary>
        /// Player Remove Event
        /// </summary>
        public static Action<PlayerBase>? PlayerRemove;

        /// <summary>
        /// Player Profile Change Event
        /// </summary>
        public static Action<PlayerBase>? ProfileChange;
        #endregion
    }
}
