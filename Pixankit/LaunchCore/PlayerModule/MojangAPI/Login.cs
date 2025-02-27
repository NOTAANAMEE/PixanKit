using PixanKit.LaunchCore.Server;
using PixanKit.LaunchCore.Server.Servers.Microsoft;
using PixanKit.LaunchCore.Server.Servers.Mojang;

namespace PixanKit.LaunchCore.PlayerModule.MojangAPI
{
    /// <summary>
    /// Standard Mojang Login Process
    /// </summary>
    public static class MojangLogin
    {
        private static MSLoginServer _msserver { get => ServerList.MicrosoftLoginServer; }

        private static XboxServer _xboxserver { get => ServerList.XboxLoginServer; }

        private static XSTSServer _xstsserver { get => ServerList.XSTSServer; }

        private static MojangLoginServer _loginserver { get => ServerList.MojangLoginServer; }

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
            => await _msserver.Authorize(code);

        /// <summary>
        /// Refresh MS Token
        /// </summary>
        /// <param name="refreshtoken">
        /// <see cref="MSLoginServer.MSAuthorize.MSrefreshToken"/>
        /// </param>
        /// <returns>
        /// <see cref="MSLoginServer.MSAuthorize"/></returns>
        public static async Task<MSLoginServer.MSAuthorize> RefreshMSToken(string refreshtoken)
            => await _msserver.ReAuthorize(refreshtoken);

        /// <summary>
        /// Get XBOX Authorize
        /// </summary>
        /// <param name="MSaccessToken">
        /// <see cref="MSLoginServer.MSAuthorize.MSaccessToken"/></param>
        /// <returns>
        /// <see cref="XboxServer.XboxAuthorize"/></returns>
        public static async Task<XboxServer.XboxAuthorize> XBoxAuthorize(string MSaccessToken)
            => await _xboxserver.Authorize(MSaccessToken);

        /// <summary>
        /// XSTS Verification Process
        /// </summary>
        /// <param name="XboxToken"><see cref="XboxServer.XboxAuthorize.Xboxtoken"/></param>
        /// <returns><see cref="XSTSServer.XSTSVerification"/></returns>
        public static async Task<XSTSServer.XSTSVerification> XSTSVerification(string XboxToken)
            => await _xstsserver.XSTSVerify(XboxToken);

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
