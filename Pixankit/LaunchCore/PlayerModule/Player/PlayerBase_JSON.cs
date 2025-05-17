using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.PlayerModule.Player;

public partial class PlayerBase
{
    /// <inheritdoc/>
    public virtual void LoadFromJson(JObject jData)
    {
        _uid = (jData["uid"] ?? "").ToString();
        _name = (jData["name"] ?? "").ToString();
        _accessToken = (jData["accesstoken"] ?? "").ToString();
    }

    /// <summary>
    /// Converts the player's data to a JSON object.
    /// </summary>
    /// <returns>A <see cref="JObject"/> representing the player's data.</returns>
    public virtual JObject ToJson()
    {
        JObject jobj = new()
        {
            { "uid", _uid },
            { "name", _name },
            { "accesstoken", _accessToken },
            { "type", Type.ToString()}
        };
        return jobj;
    }
}