using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.PlayerModule.MojangAPI;

namespace PixanKit.LaunchCore.PlayerModule.Player;

/// <summary>
/// Represents a Microsoft account player in the Minecraft environment.
/// </summary>
public class MicrosoftPlayer : PlayerBase
{
    /// <summary>
    /// Gets the latest login time. Re-login is required after 1 day.
    /// </summary>
    public DateTime LatestLoginTime  => _latestLoginTime;

    /// <summary>
    /// Gets the URL of the player's skin. Different images correspond to different URLs.
    /// </summary>
    public string SkinUrl => _skinUrl; 

    /// <summary>
    /// Gets the URL of the player's cape.
    /// </summary>
    public string CapeUrl => _capeUrl; 

    /// <summary>
    /// Gets the local cache path of the skin. It automatically updates if the <see cref="SkinUrl"/> changes.
    /// </summary>
    public string SkinCachePath => Files.SkinCacheDir + $"/{Uid}-skin.png";

    private DateTime _latestLoginTime;

    private string _skinUrl = "";

    private string _capeUrl = "";

    private string _refreshtoken = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftPlayer"/> class using JSON data.
    /// </summary>
    /// <param name="jData">The JSON data representing the player.</param>
    public MicrosoftPlayer(JObject jData) : base(jData)
    {
        Type = PlayerType.Microsoft;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftPlayer"/> class for internal use.
    /// </summary>
    protected MicrosoftPlayer() { Type = PlayerType.Microsoft; }

    /// <summary>
    /// Logs in a player using a login code.
    /// </summary>
    /// <param name="loginCode">The login code for Microsoft authentication.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="MicrosoftPlayer"/> as the result.</returns>
    public static async Task<MicrosoftPlayer> Login(string loginCode)
    {
        var ret1 = await MojangLogin.GetMsToken(loginCode);
        var ret2 = await MojangLogin.XBoxAuthorize(ret1.MSaccessToken);
        var ret3 = await MojangLogin.XstsVerification(ret2.Xboxtoken);
        var ret4 = await MojangLogin.MinecraftAccessToken(ret3);
        var ret5 = await MojangLogin.MinecraftUid(ret4);
        MicrosoftPlayer player = new()
        {
            _name = ret5.Name,
            _uid = ret5.Uid,
            _skinUrl = ret5.SkinUrl,
            _accessToken = ret4,
            _refreshtoken = ret1.MSrefreshToken,
            _latestLoginTime = DateTime.Now,
            Type = PlayerType.Microsoft,
        };
        player._capeUrl = await MojangSkin.GetCapeUrl(player);
        return player;
    }

    /// <summary>
    /// Re-logs in the player using the refresh token.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ReLogin()
    {
        var span = DateTime.Now - LatestLoginTime;
        if (span.Days >= 1)
        {
            var ret1 = await MojangLogin.RefreshMsToken(_refreshtoken);
            var ret2 = await MojangLogin.XBoxAuthorize(ret1.MSaccessToken);
            var ret3 = await MojangLogin.XstsVerification(ret2.Xboxtoken);
            _accessToken = await MojangLogin.MinecraftAccessToken(ret3);
            _refreshtoken = ret1.MSrefreshToken;
        }
        var ret5 = await MojangLogin.MinecraftUid(_accessToken);
        _name = ret5.Name;
        _uid = ret5.Uid;
        if (_skinUrl == ret5.SkinUrl && File.Exists(SkinCachePath)) return;
        _skinUrl = ret5.SkinUrl;
        await RefreshSkinCache();
    }

    /// <summary>
    /// Refreshes the local skin cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RefreshSkinCache()
    {
        HttpClient client = new();
        var response = await client.GetStreamAsync(_skinUrl);
        FileStream fs = new(
            SkinCachePath, FileMode.Create);
        response.CopyTo(fs);
        fs.Close();
        response.Dispose();
    }

    /// <inheritdoc/>
    public override void LoadFromJson(JObject jData)
    {
        base.LoadFromJson(jData);
        _latestLoginTime =
            jData.GetValue(Format.ToDateTime, "logintime");
        _skinUrl =
            jData.GetValue(Format.ToString, "skinurl");
        _capeUrl =
            jData.GetValue(Format.ToString, "capeurl");
        _refreshtoken =
            jData.GetValue(Format.ToString, "refreshtoken");
    }

    /// <summary>
    /// Converts the player's data to a JSON object.
    /// </summary>
    /// <returns>A <see cref="JObject"/> representing the player's data.</returns>
    public override JObject ToJson()
    {
        var jobj = base.ToJson();
        jobj.Add("logintime", _latestLoginTime);
        jobj.Add("skinurl", _skinUrl);
        jobj.Add("capeurl", _capeUrl);
        jobj.Add("refreshtoken", _refreshtoken);
        return jobj;
    }
}