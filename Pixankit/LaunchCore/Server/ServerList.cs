using PixanKit.LaunchCore.Server.Servers.Microsoft;
using PixanKit.LaunchCore.Server.Servers.Mojang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server
{
    public static class ServerList
    {
        public static MSLoginServer MicrosoftLoginServer = new();

        public static XboxServer XboxLoginServer = new();

        public static XSTSServer XSTSServer = new();

        public static MojangLoginServer MojangLoginServer = new();

        public static MinecraftVersionServer MinecraftVersionServer = new();

        public static MinecraftAssetsServer MinecraftAssetsServer = new();
    }

    /// <summary>
    /// Mirror Server
    /// </summary>
    /// <param name="OriginURL">The Url Part That Needs To Replace(If No, Make It "")</param>
    /// <param name="ReplacedURL">The Url That Replacing(Include https://)</param>
    public record MirrorServer(string OriginURL, string ReplacedURL);
}
