using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.Microsoft
{
    /// <summary>
    /// The Microsoft Certification Server
    /// </summary>
    public class MSLoginServer
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
        public static string ClientID = "";

        /// <summary>
        /// The Redirect URI
        /// <br/>
        /// this will not be redirected during certification process
        /// <br/>
        /// Default is https://localhost:8080<br/>
        /// <seealso href="https://help.minecraft.net/hc/en-us/articles/16254801392141">Java Game Service API</seealso>
        /// </summary>
        public static string RedirectURL = "https://localhost:8080";

        /// <summary>
        /// The Record Class For Microsoft Login
        /// </summary>
        /// <param name="MSaccessToken">accesstoken From Microsoft</param>
        /// <param name="MSrefreshToken">refreshtoken From Microsoft</param>
        public record MSAuthorize(string MSaccessToken, string MSrefreshToken);

        /// <summary>
        /// The HTTP Client
        /// </summary>
        public HttpClient Client { get; } = new HttpClient() { BaseAddress = new Uri("https://login.live.com") };

        /// <summary>
        /// The Base URL Of The Server
        /// </summary>
        public string BaseURL { get; } = "https://login.live.com";


        internal async Task<MSAuthorize> Authorize(string code)
        {
            Dictionary<string, string> data = new()
            {
                { "client_id", ClientID },//00000000402b5328
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", RedirectURL },
                { "scope", "XboxLive.signin offline_access" }
            };
            HttpContent content = new FormUrlEncodedContent(data);
            content.Headers.Add("Content_Type", "application/x-www-form-urlencoded");
            HttpResponseMessage response = await Client.PostAsync("/oauth20_token.srf", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jresponse = JObject.Parse(responseContent);
            return new MSAuthorize(
                jresponse["access_token"]?.ToString() ?? "", 
                jresponse["refresh_token"]?.ToString() ?? "");
        }

        internal async Task<MSAuthorize> ReAuthorize(string code)
        {
            if (ClientID == "" || RedirectURL == "") 
                throw new InvalidOperationException(
                    "According To New EULA Of Minecraft, Please Complete ClientID and RedirectURL");
            Dictionary<string, string> data = new()
            {
                { "client_id", ClientID },//"00000000402b5328"
                { "code", code },
                { "grant_type", "refresh_token" },
                { "redirect_uri", RedirectURL },//"https://login.live.com/oauth20_desktop.srf"
                { "scope", "XboxLive.signin offline_access" },
                { "refresh_token", code }
            };
            HttpContent content = new FormUrlEncodedContent(data);
            content.Headers.Add("Content_Type", "application/x-www-form-urlencoded");
            HttpResponseMessage response = await Client.PostAsync("/oauth20_token.srf", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jresponse = JObject.Parse(responseContent);
            return new MSAuthorize(
                jresponse["access_token"]?.ToString() ?? "", 
                jresponse["refresh_token"]?.ToString() ?? "");
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
            => Client?.Dispose();
    }

}
