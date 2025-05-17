using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.Microsoft;

/// <summary>
/// The Microsoft Certification Server
/// </summary>
public class MsLoginServer
{
    /// <summary>
    /// The Azure Client ID
    /// <br/>
    /// Developers have to apply for an 
    ///<see href="https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps">Azure Client ID</see>
    /// before publishing.
    /// <br/>
    /// <seealso href="https://help.minecraft.net/hc/en-us/articles/16254801392141">Java Game Service API</seealso>
    /// </summary>
    public static string ClientId = "";

    /// <summary>
    /// The Redirect URI
    /// <br/>
    /// this will not be redirected during certification process
    /// <br/>
    /// Default is https://localhost:8080<br/>
    /// <seealso href="https://help.minecraft.net/hc/en-us/articles/16254801392141">Java Game Service API</seealso>
    /// </summary>
    public static string RedirectUrl = "https://localhost:8080";

    /// <summary>
    /// The Record Class For Microsoft Login
    /// </summary>
    /// <param name="MSaccessToken">accesstoken From Microsoft</param>
    /// <param name="MSrefreshToken">refreshtoken From Microsoft</param>
    public record MsAuthorize(string MSaccessToken, string MSrefreshToken);

    /// <summary>
    /// The HTTP Client
    /// </summary>
    public HttpClient Client { get; } = new HttpClient() { BaseAddress = new Uri("https://login.live.com") };

    /// <summary>
    /// The Base URL Of The Server
    /// </summary>
    public string BaseUrl { get; } = "https://login.live.com";


    internal async Task<MsAuthorize> Authorize(string code)
    {
        Dictionary<string, string> data = new()
        {
            { "client_id", ClientId },//00000000402b5328
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", RedirectUrl },
            { "scope", "XboxLive.signin offline_access" }
        };
        HttpContent content = new FormUrlEncodedContent(data);
        content.Headers.Add("Content_Type", "application/x-www-form-urlencoded");
        var response = await Client.PostAsync("/oauth20_token.srf", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var jresponse = JObject.Parse(responseContent);
        return new MsAuthorize(
            jresponse["access_token"]?.ToString() ?? "",
            jresponse["refresh_token"]?.ToString() ?? "");
    }

    internal async Task<MsAuthorize> ReAuthorize(string code)
    {
        if (ClientId == "" || RedirectUrl == "")
            throw new InvalidOperationException(
                "According To New EULA Of Minecraft, Please Complete ClientID and RedirectURL");
        Dictionary<string, string> data = new()
        {
            { "client_id", ClientId },//"00000000402b5328"
            { "code", code },
            { "grant_type", "refresh_token" },
            { "redirect_uri", RedirectUrl },//"https://login.live.com/oauth20_desktop.srf"
            { "scope", "XboxLive.signin offline_access" },
            { "refresh_token", code }
        };
        HttpContent content = new FormUrlEncodedContent(data);
        content.Headers.Add("Content_Type", "application/x-www-form-urlencoded");
        var response = await Client.PostAsync("/oauth20_token.srf", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var jresponse = JObject.Parse(responseContent);
        return new MsAuthorize(
            jresponse["access_token"]?.ToString() ?? "",
            jresponse["refresh_token"]?.ToString() ?? "");
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
        => Client.Dispose();

    /// <summary>
    /// Finalizer
    /// </summary>
    ~MsLoginServer()
        => Dispose();
}