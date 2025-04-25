using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.PlayerModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Core
{
    public class PlayerManager
    {
        #region Singleton
        private static readonly Lazy<PlayerManager> _instance = new(() => new());

        public static PlayerManager Instance => _instance.Value;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of players added to the launcher.
        /// </summary>
        public PlayerBase[] Players
        {
            get => [.. _players];
        }

        private List<PlayerBase> _players = [];

        /// <summary>
        /// Gets or sets the default player used for launching.
        /// </summary>
        public PlayerBase? TargetPlayer { get; set; }
        #endregion

        #region Initor
        private PlayerManager()
        {
            // Initialize the player manager
            InitPlayerModule();
        }

        private void InitPlayerModule()
        {
            List<PlayerBase> players = [];
            foreach (JToken jData in Files.PlayerJData["children"] ?? new JArray())
            {
                PlayerBase ret = Initors.PlayerInitor((JObject)jData) ?? throw new NullReferenceException();
                players.Add(ret);
                OnPlayerLoaded?.Invoke(ret);
            }
            _players = players;
            string targetID = Files.PlayerJData["target"]?.ToString() ?? "";
            if (targetID != "") TargetPlayer = FindPlayer(targetID);
            ResetTargetPlayer();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new player to the launcher.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <exception cref="ArgumentException">Thrown if a player with the same UID already exists.</exception>
        public void AddPlayer(PlayerBase player)
        {
            foreach (PlayerBase p in Players)
            {
                if (p == player)
                    throw new ArgumentException("Player Has Added");
            }
            _players.Add(player);
            OnPlayerAdded?.Invoke(player);
            Logger.Info($"Player {player.Name} Added");
            ResetTargetPlayer();
        }

        /// <summary>
        /// Removes a player from the launcher.
        /// </summary>
        /// <param name="player">The player to remove.</param>
        public void RemovePlayer(PlayerBase player)
        { 
            _players.Remove(player); 
            OnPlayerRemoved?.Invoke(player);
            Logger.Info($"Player {player.Name} Removed");
        }

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
            if (TargetPlayer == null && _players.Count != 0)
                TargetPlayer = _players.First();
            OnTargetPlayerChanged?.Invoke(TargetPlayer);
        }

        internal JObject Save()
        {
            JArray players = [];
            foreach (var player in _players)
            {
                players.Add(player.ToJSON());
            }
            return new JObject()
            {
                { "children", players},
                { "target", TargetPlayer?.UID ?? "" }
            };
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a player profile is loaded from a JSON file.
        /// </summary>
        public static Action<PlayerBase>? OnPlayerLoaded;

        /// <summary>
        /// Occurs when a new player is added.
        /// </summary>
        public static Action<PlayerBase>? OnPlayerAdded;

        /// <summary>
        /// Occurs when a player is removed.
        /// </summary>
        public static Action<PlayerBase>? OnPlayerRemoved;

        /// <summary>
        /// Occurs when a player's profile is changed.
        /// </summary>
        public static Action<PlayerBase>? OnProfileChanged;

        public static Action<PlayerBase?>? OnTargetPlayerChanged;
        #endregion
    }
}
