using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.Microsoft;

/// <summary>
/// XBOX Authorize Server
/// </summary>
public class XboxServer
{
    /// <summary>
    /// The XBox Authorize Record Class
    /// </summary>
    /// <param name="Xboxtoken">XBox token</param>
    /// <param name="UserHash">User Hash Used To Verify The User's Ientity</param>
    public record XboxAuthorize(string Xboxtoken, string UserHash);

    /// <summary>
    /// The HTTP Client
    /// </summary>
    public HttpClient Client { get; } = new HttpClient() { BaseAddress = new Uri("https://user.auth.xboxlive.com") };

    /// <summary>
    /// The Base URL
    /// </summary>
    public string BaseUrl { get; } = "https://user.auth.xboxlive.com";

    internal async Task<XboxAuthorize> Authorize(string mSaccessToken)
    {
        string jsonData, ret;
        //初始化jsonData
        {
            jsonData =
                "{\"Properties\":" +
                "{\"AuthMethod\":\"RPS\"," +
                "\"SiteName\":\"user.auth.xboxlive.com\"," +
                "\"RpsTicket\":\"" + mSaccessToken + "\"" +
                "}," +
                "\"RelyingParty\":\"http://auth.xboxlive.com\"," +
                "\"TokenType\":\"JWT\"" +
                "}";
        }

        //发送数据

        HttpContent content = new StringContent(jsonData);
        content.Headers.Clear();
        content.Headers.Add("Content-Type", "application/json");
        var response = await Client.PostAsync("/user/authenticate", content);
        ret = await response.Content.ReadAsStringAsync();
        var jresponse = JObject.Parse(ret);
        return new
        (jresponse["Token"]?.ToString() ?? "",
            jresponse["DisplayClaims"]?["xui"]?[0]?["uhs"]?.ToString() ?? "");
    }

    /// <summary>
    /// Dispose The Client
    /// </summary>
    public void Dispose()
        => Client.Dispose();

    /// <summary>
    /// Finalizer
    /// </summary>
    ~XboxServer()
        => Dispose();
}