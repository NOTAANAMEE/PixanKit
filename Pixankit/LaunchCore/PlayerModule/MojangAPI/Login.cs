using PixanKit.LaunchCore.Server;
using PixanKit.LaunchCore.Server.Servers.Microsoft;
using PixanKit.LaunchCore.Server.Servers.Mojang;

namespace PixanKit.LaunchCore.PlayerModule.MojangAPI
{
    /// <summary>
    /// This class implements the Microsoft OAuth authorization code login functionality.
    /// </summary>
    /// <remarks>
    /// Microsoft has changed the login verification method. You need to register an application on Azure and obtain a Client ID.<br/>
    /// Before performing the login operation, you must provide your Client ID and Redirect URL.<br/>
    /// Since the developer does not have the time and resources to apply for a Client ID
    /// and request application approval from Mojang, this class has not been tested.
    /// We sincerely ask developers to help with testing.<br/>
    /// The good news is that before Mojang changes the 
    /// <see href="https://help.minecraft.net/hc/en-us/articles/16254801392141">application approval process</see>,
    /// this class should work as expected.<br/>
    /// </remarks>
    public static class MojangLogin
    {
        private static MSLoginServer Msserver => ServerList.MicrosoftLoginServer;

        private static XboxServer Xboxserver => ServerList.XboxLoginServer;

        private static XSTSServer Xstsserver => ServerList.XSTSServer; 

        private static MojangLoginServer _loginserver => ServerList.MojangLoginServer;

        /// <summary>
        /// Verify Microsoft Account From The Code
        /// </summary>
        /// <param name="code">
        /// The Code From The Browser<br/>
        /// For example: 
        /// <see href="https://login.live.com/oauth20_desktop.srf?code=M.C529_BL2.2.U.AAAAAAAAAAAAAAAAA...."/> 
        /// Code:M.C529_BL2.2.U.AAAAAAAAAAAAAAAAA
        /// </param>
        /// <returns>
        /// <see cref="MSLoginServer.MSAuthorize"/>
        /// </returns>
        public static async Task<MSLoginServer.MSAuthorize> GetMSToken(string code)
            => await Msserver.Authorize(code);

        /// <summary>
        /// Refresh MS Token
        /// </summary>
        /// <param name="refreshtoken">
        /// <see cref="MSLoginServer.MSAuthorize.MSrefreshToken"/>
        /// </param>
        /// <returns>
        /// <see cref="MSLoginServer.MSAuthorize"/></returns>
        public static async Task<MSLoginServer.MSAuthorize> RefreshMSToken(string refreshtoken)
            => await Msserver.ReAuthorize(refreshtoken);

        /// <summary>
        /// Get XBOX Authorize
        /// </summary>
        /// <param name="MSaccessToken">
        /// <see cref="MSLoginServer.MSAuthorize.MSaccessToken"/></param>
        /// <returns>
        /// <see cref="XboxServer.XboxAuthorize"/></returns>
        public static async Task<XboxServer.XboxAuthorize> XBoxAuthorize(string MSaccessToken)
            => await Xboxserver.Authorize(MSaccessToken);

        /// <summary>
        /// XSTS Verification Process
        /// </summary>
        /// <param name="XboxToken"><see cref="XboxServer.XboxAuthorize.Xboxtoken"/></param>
        /// <returns><see cref="XSTSServer.XSTSVerification"/></returns>
        public static async Task<XSTSServer.XSTSVerification> XSTSVerification(string XboxToken)
            => await Xstsserver.XSTSVerify(XboxToken);

        /// <summary>
        /// Get The Minecraft AccessToken
        /// </summary>
        /// <param name="verification"></param>
        /// <returns>The Minecraft AccessToken</returns>
        public static async Task<string> MinecraftAccessToken(XSTSServer.XSTSVerification verification)
            => await _loginserver.GetAccessToken(verification);

        /// <summary>
        /// Get uuid,Name And Skin url
        /// </summary>
        /// <param name="accessToken">Minecraft Access Token</param>
        /// <returns><see cref="MojangLoginServer.PlayerInf"/></returns>
        /// <exception cref="InvalidOperationException">If The User Has Not Bought Minecraft Yet, The Exception Will Be Thrown</exception>
        public static async Task<MojangLoginServer.PlayerInf> MinecraftUid(string accessToken)
            => await _loginserver.GetPlayerInformation(accessToken);
    }
}
