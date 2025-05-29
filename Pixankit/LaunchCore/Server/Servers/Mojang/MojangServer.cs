using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server.Servers.Microsoft;
using System.Net.Http.Headers;

namespace PixanKit.LaunchCore.Server.Servers.Mojang;

/// <summary>
/// Mojang Login Server
/// </summary>
public class MojangLoginServer
{
    /// <summary>
    /// The Record Class For Player Information
    /// </summary>
    /// <param name="Uid">Player ID</param>
    /// <param name="Name">Player Name</param>
    /// <param name="SkinUrl">Player Skin</param>
    public record PlayerInf(string Uid, string Name, string SkinUrl);

    /// <summary>
    /// The Base URL Of The Server
    /// </summary>
    public string BaseUrl { get; } = "https://api.minecraftservices.com";

    /// <summary>
    /// The HTTP Client
    /// </summary>
    public HttpClient Client { get; } = new HttpClient() { BaseAddress = new Uri("https://api.minecraftservices.com") };

    internal async Task<string> GetAccessToken(XstsServer.XstsVerification authorize)
    {
        string data, ret;
        {
            data =
                "{\"identityToken\":\"XBL3.0 x=" + authorize.UserHash + ";" + authorize.Xststoken + "\"}";
        }
        HttpContent content = new StringContent(data);
        content.Headers.Clear();
        content.Headers.Add("Content-Type", "application/json");
        //content.Headers.Add("User-Agent", "");
        var response = await Client.PostAsync("/authentication/login_with_xbox", content);
        ret = await response.Content.ReadAsStringAsync();
        var jresponse = JObject.Parse(ret);
        return jresponse["access_token"]?.ToString() ?? "";
    }

    internal async Task<PlayerInf> GetPlayerInformation(string accessToken)
    {
        string ret;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await Client.GetAsync($"/minecraft/profile");
        ret = await response.Content.ReadAsStringAsync();

        var jresponse = JObject.Parse(ret);
        if (jresponse.ContainsKey("error")) throw new InvalidOperationException("Player Does Not Have Minecraft");
        return new PlayerInf(jresponse["id"]?.ToString() ?? "",
            jresponse["name"]?.ToString() ?? "",
            jresponse["skins"]?[0]?["url"]?.ToString() ?? "");
    }

    /// <summary>
    /// Dispose The HTTP Client
    /// </summary>
    public void Dispose()
        => Client?.Dispose();
}