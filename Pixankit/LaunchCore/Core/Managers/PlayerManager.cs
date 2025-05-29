using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.PlayerModule.Player;

namespace PixanKit.LaunchCore.Core.Managers;

/// <summary>
/// Manages player profiles for the launcher, including adding, removing, and retrieving players.
/// </summary>
public class PlayerManager
{
    #region Properties
    /// <summary>
    /// Gets the collection of players added to the launcher.
    /// </summary>
    public PlayerBase[] Players => [.. _players];

    private List<PlayerBase> _players = [];

    private PlayerBase? _targetPlayer;

    /// <summary>
    /// Gets or sets the default player used for launching.
    /// </summary>
    public PlayerBase? TargetPlayer 
    { get => _targetPlayer;
        set
        {
            _targetPlayer = value;
            OnTargetPlayerChanged?.Invoke(_targetPlayer);
        }
    }
    #endregion

    #region Initor
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerManager"/> class.
    /// </summary>
    internal PlayerManager()
    { }

    /// <summary>
    /// Initializes the player module by loading player data from a JSON file.
    /// </summary>
    public void InitPlayerModule()
    {
        List<PlayerBase> players = [];
        foreach (var jData in Files.PlayerJData["children"] ?? new JArray())
        {
            var ret = Initers.PlayerIniter((JObject)jData) ?? throw new NullReferenceException();
            players.Add(ret);
            OnPlayerLoaded?.Invoke(ret);
        }
        _players = players;
        var targetId = Files.PlayerJData["target"]?.ToString() ?? "";
        if (targetId != "") TargetPlayer = FindPlayer(targetId);
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
        foreach (var p in Players)
        {
            if (p == player)
                throw new ArgumentException("Player Has Added");
        }
        _players.Add(player);
        ResetTargetPlayer();
        OnPlayerAdded?.Invoke(player);
        Logger.Logger.Info($"Player {player.Name} Added");
        ResetTargetPlayer();
    }

    /// <summary>
    /// Removes a player from the launcher.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    public void RemovePlayer(PlayerBase player)
    { 
        _players.Remove(player); 
        if (player == _targetPlayer)
            _targetPlayer = null;
        OnPlayerRemoved?.Invoke(player);
        ResetTargetPlayer();
        Logger.Logger.Info($"Player {player.Name} Removed");
    }

    /// <summary>
    /// Finds a player by their unique identifier (UID).
    /// </summary>
    /// <param name="uid">The unique identifier of the player.</param>
    /// <returns>The player if found; otherwise, <c>null</c>.</returns>
    public PlayerBase? FindPlayer(string uid)
    {
        foreach (var player in _players)
        {
            if (player.Uid == uid) return player;
        }
        return null;
    }

    /// <summary>
    /// Resets the target player to the first player in the list if no target player is set.
    /// </summary>
    private void ResetTargetPlayer()
    {
        if (TargetPlayer == null)
            TargetPlayer = _players.FirstOrDefault();
    }

    /// <summary>
    /// Saves the current player data to a JSON object.
    /// </summary>
    /// <returns>A <see cref="JObject"/> containing the player data.</returns>
    internal JObject Save()
    {
        JArray players = [];
        foreach (var player in _players)
        {
            players.Add(player.ToJson());
        }
        return new JObject()
        {
            { "children", players},
            { "target", TargetPlayer?.Uid ?? "" }
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

    /// <summary>
    /// Occurs when the target player is changed.
    /// </summary>
    public static Action<PlayerBase?>? OnTargetPlayerChanged;
    #endregion
}