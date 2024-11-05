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
        /// <summary>
        /// Player Collection
        /// </summary>
        public PlayerBase[] Players
        {
            get => _players.ToArray();
        }

        private List<PlayerBase> _players = new();

        /// <summary>
        /// Default Launch Player
        /// </summary>
        public PlayerBase? TargetPlayer { get; set; }

        /// <summary>
        /// Inline Player Information To Command
        /// </summary>
        /// <param name="arg">The Command</param>
        /// <param name="player">Player Used To Launch</param>
        /// <returns>Command Line After Inline</returns>
        /// <exception cref="ArgumentNullException">Player Should Not Be Null</exception>
        public string PlayerInLine(string arg, PlayerBase? player)
        {
            if (player == null) throw new ArgumentNullException("");
            return player.InlinePlayer(arg);
        }

        /// <summary>
        /// Inline Default Player Information To Command
        /// </summary>
        /// <param name="arg">The Command</param>
        /// <returns>The Command After Inline</returns>
        public string PlayerInLine(string arg)
            =>PlayerInLine(arg, TargetPlayer);

        /// <summary>
        /// Add A New Player To The Launcher
        /// </summary>
        /// <param name="player">Player Needed To Add</param>
        /// <exception cref="ArgumentException">If Player UID Exists, The Exception Is Raised</exception>
        public void AddPlayer(PlayerBase player)
        {
            foreach (var p in Players) 
            {
                if (p == player) 
                    throw new ArgumentException("Player Has Added");
            }
            _players.Add(player);
            ResetTargetPlayer();
        }

        /// <summary>
        /// Remove Player
        /// </summary>
        /// <param name="player">Player Needed To Be Removed</param>
        public void RemovePlayer(PlayerBase player)
            => _players.Remove(player);

        /// <summary>
        /// Get The Player From UID
        /// </summary>
        /// <param name="uid">Player UID</param>
        /// <returns>If Exists, Return The Player. Else Return null</returns>
        public PlayerBase? FindPlayer(string uid)
        {
            foreach (PlayerBase player in _players)
            {
                if (player.UID == uid) return player;
            }
            return null;
        }

        private void ResetTargetPlayer()
        {
            if (TargetPlayer != null || _players.Count == 0) return;
            TargetPlayer = _players.First();
        }
    }
}
