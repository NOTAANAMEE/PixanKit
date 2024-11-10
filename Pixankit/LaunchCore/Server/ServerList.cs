using PixanKit.LaunchCore.Server.Servers.Microsoft;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
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

        /// <summary>
        /// Mod Loader Servers
        /// </summary>
        public static Dictionary<string, ModLoaderServer> ModLoaderServers = new();
    }

    
}
