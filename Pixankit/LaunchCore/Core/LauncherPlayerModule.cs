using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.PlayerModule.Player;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Gets the collection of players added to the launcher.
        /// </summary>
        public PlayerBase[] Players
        {
            get => [.._players];
        }

        private List<PlayerBase> _players = [];

        /// <summary>
        /// Gets or sets the default player used for launching.
        /// </summary>
        public PlayerBase? TargetPlayer { get; set; }

        /// <summary>
        /// Inlines player information into a command string.
        /// </summary>
        /// <param name="arg">The base command string.</param>
        /// <param name="player">The player whose information is to be inlined.</param>
        /// <returns>The command string with the player's information inlined.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided player is null.</exception>
        public string PlayerInLine(string arg, PlayerBase? player)
        {
            return player == null ? 
                throw new ArgumentNullException("Player Should Not Be Null") : 
                player.InlinePlayer(arg);
        }

        /// <summary>
        /// Inlines the default player's information into a command string.
        /// </summary>
        /// <param name="arg">The base command string.</param>
        /// <returns>The command string with the default player's information inlined.</returns>
        public string PlayerInLine(string arg)
            =>PlayerInLine(arg, TargetPlayer);

        /// <summary>
        /// Adds a new player to the launcher.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <exception cref="ArgumentException">Thrown if a player with the same UID already exists.</exception>
        public void AddPlayer(PlayerBase player)
        {
            foreach (var p in Players) 
            {
                if (p == player) 
                    throw new ArgumentException("Player Has Added");
            }
            _players.Add(player);
            Logger.Info($"Player {player.Name} Added");
            ResetTargetPlayer();
        }

        /// <summary>
        /// Removes a player from the launcher.
        /// </summary>
        /// <param name="player">The player to remove.</param>
        public void RemovePlayer(PlayerBase player)
            => _players.Remove(player);

        /// <summary>
        /// Finds a player by their unique identifier (UID).
        /// </summary>
        /// <param name="uid">The unique identifier of the player.</param>
        /// <returns>The player if found; otherwise, <c>null</c>.</returns>
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
