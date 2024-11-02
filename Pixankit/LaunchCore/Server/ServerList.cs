using PixanKit.LaunchCore.Server.Servers.Microsoft;
using PixanKit.LaunchCore.Server.Servers.Mojang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server
{
    /// <summary>
    /// Collection Of Servers
    /// </summary>
    public static class ServerList
    {
        /// <summary>
        /// Server Instance For Microsoft Account
        /// </summary>
        public static MSLoginServer MicrosoftLoginServer = new();

        /// <summary>
        /// Server Instance For XBox
        /// </summary>
        public static XboxServer XboxLoginServer = new();

        /// <summary>
        /// Server Instance For XSTS
        /// </summary>
        public static XSTSServer XSTSServer = new();

        /// <summary>
        /// Server Instance For Mojang Account
        /// </summary>
        public static MojangLoginServer MojangLoginServer = new();

        /// <summary>
        /// Server Instance For Minecraft Versions
        /// </summary>
        public static MinecraftVersionServer MinecraftVersionServer = new();

        /// <summary>
        /// Server Instance For Minecraft Assets
        /// </summary>
        public static MinecraftAssetsServer MinecraftAssetsServer = new();
    }

    /// <summary>
    /// Mirror Server
    /// </summary>
    /// <param name="OriginURL">The Url Part That Needs To Replace(If No, Make It "")</param>
    /// <param name="ReplacedURL">The Url That Replacing(Include https://)</param>
    public record MirrorServer(string OriginURL, string ReplacedURL);
}
