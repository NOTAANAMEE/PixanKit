using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server.Servers.Microsoft
{
    /// <summary>
    /// XSTS Verification Server
    /// </summary>
    public class XSTSServer
    {
        /// <summary>
        /// The Record Class For XSTS Verify Result
        /// </summary>
        /// <param name="Xststoken">The XSTS Token</param>
        /// <param name="UserHash">User Hash</param>
        public record XSTSVerification(string Xststoken, string UserHash);

        /// <summary>
        /// The HTTP Client
        /// </summary>
        public HttpClient Client { get; } = new HttpClient() { BaseAddress = new Uri("https://xsts.auth.xboxlive.com") };

        /// <summary>
        /// Base URL Of The Server
        /// </summary>
        public string BaseURL { get; } = "https://xsts.auth.xboxlive.com";

        internal async Task<XSTSVerification> XSTSVerify(string XboxToken)
        {
            string data, ret;
            {
                data =
                    "{\"Properties\":" +
                        "{\"SandboxId\":\"RETAIL\"," +
                         "\"UserTokens\":" +
                            "[" +
                            "\"" + XboxToken + "\"" +
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
            JObject jresponse = JObject.Parse(ret);
            return new XSTSVerification(jresponse["Token"]?.ToString() ?? "",
                jresponse["DisplayClaims"]?["xui"]?[0]?["uhs"]?.ToString() ?? "");
        }

        /// <summary>
        /// Dispose The Client
        /// </summary>
        public void Dispose()
            => Client?.Dispose();
    }
}
