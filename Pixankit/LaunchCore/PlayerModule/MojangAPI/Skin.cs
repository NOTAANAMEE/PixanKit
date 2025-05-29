using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.PlayerModule.Player;
using System.Net.Http.Headers;
using System.Text;

namespace PixanKit.LaunchCore.PlayerModule.MojangAPI;

/// <summary>
/// Skin Class For Launchers To Change The Skin
/// </summary>
public static class MojangSkin
{
    private static HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    /// <summary>
    /// Change the skin to steve/alex
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static async Task Delete(MicrosoftPlayer player)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", player.AccessToken);
        await _client.DeleteAsync($"https://api.mojang.com/user/profile/{player.Uid}/skin");
    }

    /// <summary>
    /// Upload the skin
    /// </summary>
    /// <param name="player"></param>
    /// <param name="skinPath"></param>
    /// <param name="slim"></param>
    /// <returns></returns>
    public static async Task Upload(MicrosoftPlayer player, string skinPath, bool slim)
    {
        if (skinPath == "") return;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", player.AccessToken);

        var skinData = File.ReadAllBytes(skinPath);

        MultipartFormDataContent content = new()
        {
            { new StringContent((slim)? "slim":"classic"), "variant" },
            { new ByteArrayContent(File.ReadAllBytes(skinPath)), "file", Path.GetFileName(skinPath) }
        };

        var response = await _client.PostAsync(
            $"https://api.minecraftservices.com/minecraft/profile/skins", content);
    }

    /// <summary>
    /// Get The Cape Code
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static async Task<string> GetCapeUrl(MicrosoftPlayer player)
    {
        var response = await _client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{player.Uid}");
        var jData = JObject.Parse(response);
        var base64Code = jData["properties"]?[0]?["value"]?.ToString() ?? "";
        jData = JObject.Parse(Base64Decode(base64Code));
        return jData["textures"]?["CAPE"]?["url"]?.ToString() ?? "";
    }

    private static string Base64Decode(string encodedString)
    {
        var bytes = Convert.FromBase64String(encodedString);
        var decodedString = Encoding.UTF8.GetString(bytes);
        return decodedString;
    }
}