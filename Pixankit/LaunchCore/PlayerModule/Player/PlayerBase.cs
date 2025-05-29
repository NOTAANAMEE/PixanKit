using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.PlayerModule.Player;

/// <summary>
/// Represents a base class for a player in the Minecraft environment.
/// </summary>
public partial class PlayerBase : IToJson
{
    /// <summary>
    /// Gets the player's name. Necessary for launch.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the player's unique identifier (UID). Necessary for launch.
    /// </summary>
    public virtual string Uid => _uid;

    /// <summary>
    /// Gets the player's access token for verification. Necessary for launch.
    /// </summary>
    public virtual string AccessToken => _accessToken;

    /// <summary>
    /// Gets the player's login type.
    /// </summary>
    public PlayerType LoginType => Type;

    /// <summary>
    /// The player's unique identifier.
    /// </summary>
    protected string _uid = "";

    /// <summary>
    /// The player's name.
    /// </summary>
    protected string _name = "";

    /// <summary>
    /// The player's login type.
    /// </summary>
    protected PlayerType Type;

    /// <summary>
    /// The player's Mojang access token.
    /// </summary>
    protected string _accessToken = "";

    /// <summary>
    /// The player's Microsoft refresh token.
    /// </summary>
    protected string _refreshToken = "";

    /// <summary>
    /// The last login time of the player.
    /// </summary>
    protected DateTime LastLogin;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerBase"/> class with the specified JSON data.
    /// </summary>
    /// <param name="jData">The JSON data of a player.</param>
    public PlayerBase(JObject? jData)
    {
        _name = (jData?["name"] ?? "").ToString();
        _uid = (jData?["uid"] ?? "").ToString();
        _accessToken = (jData?["accesstoken"] ?? "").ToString();
        if (jData == null) return;
        Logger.Logger.Info($"Player Init, Name:{Name} UID:{_uid}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerBase"/> class.
    /// </summary>
    protected PlayerBase() { }

    /// <summary>
    /// Inlines the player's data into a command string.
    /// </summary>
    /// <param name="arg">The command string.</param>
    /// <returns>The command string with the player's data inlined.</returns>
    public string InlinePlayer(string arg)
    {
        arg = arg.Replace("${auth_player_name}", Name);
        arg = arg.Replace("${auth_uuid}", Uid);
        arg = arg.Replace("${auth_access_token}", AccessToken);
        arg = arg.Replace("${user_type}", "msa");
        return arg;
    }

    /// <summary>
    /// Determines whether two players are the same.
    /// </summary>
    /// <param name="player1">The first player.</param>
    /// <param name="player2">The second player.</param>
    /// <returns><c>true</c> if the players are the same; otherwise, <c>false</c>.</returns>
    protected static bool SamePlayer(PlayerBase player1, PlayerBase player2)
    {
        return player1._uid == player2._uid;
    }

    /// <summary>
    /// Determines whether two <see cref="PlayerBase"/> objects are equal.
    /// </summary>
    /// <param name="player1">The first player.</param>
    /// <param name="player2">The second player.</param>
    /// <returns><c>true</c> if the players are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(PlayerBase? player1, PlayerBase? player2)
    {
        if (player1 is null) return player2 is null;
        else if (player2 is null) return false;
        return SamePlayer(player1, player2);
    }

    /// <summary>
    /// Determines whether two <see cref="PlayerBase"/> objects are not equal.
    /// </summary>
    /// <param name="player1">The first player.</param>
    /// <param name="player2">The second player.</param>
    /// <returns><c>true</c> if the players are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(PlayerBase? player1, PlayerBase? player2)
    {
        return !(player1 == player2);
    }

    /// <summary>
    /// Gets a hash code for the player.
    /// </summary>
    /// <returns>A hash code for the player.</returns>
    public override int GetHashCode()
    {
        return _uid.GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current player.
    /// </summary>
    /// <param name="obj">The object to compare with the current player.</param>
    /// <returns><c>true</c> if the specified object is equal to the current player; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        return ((PlayerBase)obj) == this;
    }
}

/// <summary>
/// Player Login Type
/// </summary>
public enum PlayerType
{
    /// <summary>
    /// Microsoft Login
    /// </summary>
    Microsoft,
    /// <summary>
    /// Offline Login
    /// </summary>
    Offline,
    /// <summary>
    /// Third-Party Yggdrasil Server
    /// </summary>
    Yggdrasil
}