using PixanKit.LaunchCore.Server.Servers.ModLoader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server.Servers.ModLoader
{
    /// <summary>
    /// Server Layer For Mod Loader Installer
    /// </summary>
    public abstract class ModLoaderServer:ResourceServer
    {
        /// <summary>
        /// Mirror Servers
        /// </summary>
        public new List<ModLoaderMirror> Mirrors = new();

        /// <summary>
        /// Current Server
        /// </summary>
        public new ModLoaderMirror Current;

        /// <summary>
        /// The Name Of The Mo Loader
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Init A Mod Loader Server
        /// </summary>
        public ModLoaderServer(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Check Whether Minecraft Version Is Supported
        /// </summary>
        /// <param name="minecraftversion"></param>
        /// <returns></returns>
        public Task<bool> CheckBuild(string minecraftversion)
            => Current.CheckBuild(minecraftversion);

        /// <summary>
        /// Get The URL Of The Mod Loader Installer
        /// </summary>
        /// <param name="modloaderinf"></param>
        /// <returns></returns>
        public Task<string> GetURL(JObject modloaderinf)
            => Current.GetURL(modloaderinf);
 
        /// <summary>
        /// Get The Modloader Versions That Are Suitable For The Minecraft Version
        /// </summary>
        /// <param name="minecraftversion">Minecraft Version</param>
        /// <returns>List Of The ModLoader Versions</returns>
        public Task<JArray> GetVersionsForMinecraft(string minecraftversion)
            => Current.GetBuild(minecraftversion);
    }
}
