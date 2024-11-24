using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server.Servers.ModLoader
{
    /// <summary>
    /// ModLoader Mirror Server
    /// </summary>
    public abstract class ModLoaderMirror:MirrorServer
    {
        #region CancellableMethod
        /// <summary>
        /// Check whether the builds exist
        /// </summary>
        /// <param name="mcversion">Minecraft Version</param>
        /// <param name="token">Cancel Token</param>
        /// <returns></returns>
        public abstract Task<bool> CheckBuild(string mcversion, CancellationToken token);

        /// <summary>
        /// Get The Build List
        /// </summary>
        /// <param name="mcversion">Minecraft Version</param>
        /// <param name="token">Cancel Token</param>
        /// <returns>Mod Loader Version List</returns>
        public abstract Task<JArray> GetBuild(string mcversion, CancellationToken token);

        /// <summary>
        /// Get The URL Of The Mod Loader Installer
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="modloaderinf"></param>
        /// <returns></returns>
        public abstract Task<string> GetURL(JObject modloaderinf, CancellationToken token);
        #endregion
    }
}
