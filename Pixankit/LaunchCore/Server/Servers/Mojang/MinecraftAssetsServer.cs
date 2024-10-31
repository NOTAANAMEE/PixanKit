using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server.Servers.Mojang
{
    public class MinecraftAssetsServer:ResourceServer
    {
        public MinecraftAssetsServer() 
        {
            Mirrors = new List<MirrorServer>
            { new("", "https://resources.download.minecraft.net") };
        }

        /// <summary>
        /// Get The URL Of The Asset
        /// </summary>
        /// <param name="hash">{"hash":"This is the hash parameter", "size":8964}</param>
        /// <returns>The URL</returns>
        public string GetAssetsUrl(string hash)
        {
            return $"{Current.ReplacedURL}/{hash[0..2]}/{hash}";
        }

        /// <summary>
        /// Get The URL Of The Assets
        /// </summary>
        /// <param name="jData"></param>
        /// <returns>The URL Of The Asset</returns>
        public string GetAssetsUrl(JObject jData)
        {
            string hash = jData.First["hash"].ToString();
            return $"{Current.ReplacedURL}/{hash[0..2]}/{hash}";
        }

        /// <summary>
        /// Get The Target Location Of The Asset File
        /// </summary>
        /// <param name="jData"></param>
        /// <returns></returns>
        public string GetFileLocation(JObject jData)
        {
            string hash = jData.First["hash"].ToString();
            return $"/{hash[0..2]}/{hash}";
        }
    }
}
