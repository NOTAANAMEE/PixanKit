using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.Microsoft;

/// <summary>
/// XSTS Verification Server
/// </summary>
public class XstsServer
{
    /// <summary>
    /// The Record Class For XSTS Verify Result
    /// </summary>
    /// <param name="Xststoken">The XSTS Token</param>
    /// <param name="UserHash">User Hash</param>
    public record XstsVerification(string Xststoken, string UserHash);

    /// <summary>
    /// The HTTP Client
    /// </summary>
    public HttpClient Client { get; } = new HttpClient() { BaseAddress = new Uri("https://xsts.auth.xboxlive.com") };

    /// <summary>
    /// Base URL Of The Server
    /// </summary>
    public string BaseUrl { get; } = "https://xsts.auth.xboxlive.com";

    internal async Task<XstsVerification> XstsVerify(string xboxToken)
    {
        string data, ret;
        {
            data =
                "{\"Properties\":" +
                "{\"SandboxId\":\"RETAIL\"," +
                "\"UserTokens\":" +
                "[" +
                "\"" + xboxToken + "\"" +
                "]" +
                "}," +
                "\"RelyingParty\":\"rp://api.minecraftservices.com/\"," +
                "\"TokenType\":\"JWT\"" +
                "}";
        }
        HttpContent content = new StringContent(data);
        content.Headers.Add("Content_type", "application/json");
        content.Headers.Add("Accept_type", "application/json");
        var response = await Client.PostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", content);
        ret = await response.Content.ReadAsStringAsync();
        var jresponse = JObject.Parse(ret);
        return new XstsVerification(jresponse["Token"]?.ToString() ?? "",
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
    ~XstsServer()
        => Dispose();
}