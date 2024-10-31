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
        public PlayerBase[] Players
        {
            get => _players.ToArray();
        }

        private List<PlayerBase> _players = new();

        public PlayerBase? TargetPlayer { get; set; }

        public string PlayerInLine(string arg, PlayerBase? player)
        {
            if (player == null) throw new ArgumentNullException("");
            return player.InlinePlayer(arg);
        }

        public string PlayerInLine(string arg)
            =>PlayerInLine(arg, TargetPlayer);

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

        public void RemovePlayer(PlayerBase player)
            => _players.Remove(player);

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
