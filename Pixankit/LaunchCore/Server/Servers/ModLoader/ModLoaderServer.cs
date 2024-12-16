using PixanKit.LaunchCore.Server.Servers.ModLoader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule;

namespace PixanKit.LaunchCore.Server.Servers.ModLoader
{
    /// <summary>
    /// Server Layer For Mod Loader Installer
    /// </summary>
    public abstract class ModLoaderServer:ResourceServer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="loaderversion"></param>
        /// <param name="name"></param>
        public static string Move(Folder folder, string loaderversion, string name)
        {
            string folderpath = $"{folder.VersionDir}/{name}";
            string folderpath_old = $"{folder.VersionDir}/{loaderversion}";
            string jarpath = $"{folderpath}/{name}.jar";
            string jarpath_old = $"{folderpath}/{loaderversion}.jar";
            string jsonpath = $"{folderpath}/{name}.json";
            string jsonpath_old = $"{folderpath}/{loaderversion}.json";
            Directory.Move(folderpath_old, folderpath);
            if (File.Exists(jarpath_old)) File.Move(jarpath_old, jarpath);
            if (File.Exists(jsonpath_old)) File.Move(jsonpath_old, jsonpath);
            return folderpath;
        }

        /// <summary>
        /// Current Server
        /// </summary>
        public new ModLoaderMirror? Current { get => base.Current as ModLoaderMirror;
            set => base.Current = value;
        }

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
            ServerList.ModLoaderServers.Add(name, this);
        }

        /// <summary>
        /// Check Whether Minecraft Version Is Supported
        /// </summary>
        /// <param name="minecraftversion"></param>
        /// <returns></returns>
        public Task<bool> CheckBuild(string minecraftversion)
            => CheckBuild(minecraftversion, CancellationToken.None);

        /// <summary>
        /// Check Whether Minecraft Version Is Supported
        /// </summary>
        /// <param name="cancellationToken">Cancel Token</param>
        /// <param name="minecraftversion"></param>
        /// <returns></returns>
        public Task<bool> CheckBuild(string minecraftversion, CancellationToken cancellationToken)
            => Current.CheckBuild(minecraftversion, cancellationToken);

        /// <summary>
        /// Get The URL Of The Mod Loader Installer
        /// </summary>
        /// <param name="modloaderinf"></param>
        /// <returns></returns>
        public Task<string> GetURL(JObject modloaderinf)
            => GetURL(modloaderinf, CancellationToken.None);

        public Task<string> GetURL(JObject modloaderinf, CancellationToken token)
            => Current.GetURL(modloaderinf, token);

        /// <summary>
        /// Get The Modloader Versions That Are Suitable For The Minecraft Version
        /// </summary>
        /// <param name="minecraftversion">Minecraft Version</param>
        /// <returns>List Of The ModLoader Versions</returns>
        public Task<JArray> GetVersionsForMinecraft(string minecraftversion)
            => GetVersionsForMinecraft(minecraftversion, CancellationToken.None);

        public Task<JArray> GetVersionsForMinecraft(string minecraftversion, CancellationToken token)
            => Current.GetBuild(minecraftversion, token);
    }
}
