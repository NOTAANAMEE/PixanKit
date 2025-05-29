using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.PlayerModule.Player;

/// <summary>
/// Represents an offline player in the Minecraft environment.
/// </summary>
public class OfflinePlayer : PlayerBase
{
    /// <summary>
    /// Gets the unique identifier (UID) for the offline player.
    /// </summary>
    public override string Uid => "00000FFFFFFFFFFFFFFFFFFFFFF1FF43";

    /// <summary>
    /// Gets the access token for the offline player, which is the same as the UID.
    /// </summary>
    public override string AccessToken => Uid;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflinePlayer"/> class with a specified name.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    public OfflinePlayer(string name) : base(null)
    {
        _name = name;
        _uid = $"uid{name}offline";
        _accessToken = "";
        Type = PlayerType.Offline;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflinePlayer"/> class with JSON data.
    /// </summary>
    /// <param name="jData">The JSON data representing the player.</param>
    public OfflinePlayer(JObject jData) : base(jData)
    {
        Type = PlayerType.Offline;
    }

    /// <summary>
    /// Sets the name of the offline player.
    /// </summary>
    /// <param name="name"></param>
    public void SetName(string name)
    {
        _name = name;
    }
}