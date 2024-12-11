using PixanKit.LaunchCore.PlayerModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Drawing;
using System.Numerics;
using Newtonsoft.Json.Linq;
using System.Buffers.Text;
using System.ComponentModel;

namespace PixanKit.LaunchCore.PlayerModule.MojangAPI
{
    /// <summary>
    /// Skin Class For Launchers To Change The Skin
    /// </summary>
    public static class MojangSkin
    {
        private static HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        /// <summary>
        /// Change the skin to steve/alex
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static async Task Delete(MicrosoftPlayer player) 
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", player.AccessToken);
            await client.DeleteAsync($"https://api.mojang.com/user/profile/{player.UID}/skin");
        }
        
        /// <summary>
        /// Upload the skin
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skinPath"></param>
        /// <param name="slim"></param>
        /// <returns></returns>
        public static async Task Upload(MicrosoftPlayer player, string skinPath, bool slim) 
        {
            if (skinPath == "") return;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", player.AccessToken);

            byte[] skinData = File.ReadAllBytes(skinPath);

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StringContent((slim)? "slim":"classic"), "variant" },
                { new ByteArrayContent(File.ReadAllBytes(skinPath)), "file", Path.GetFileName(skinPath) }
            };

            var response = await client.PostAsync(
                $"https://api.minecraftservices.com/minecraft/profile/skins", content);
        }

        /// <summary>
        /// Get The Cape Code
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static async Task<string> GetCapeURL(MicrosoftPlayer player)
        {
            var response = await client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{player.UID}");
            JObject jData = JObject.Parse(response);
            string base64code = jData["properties"][0]["value"].ToString();
            jData = JObject.Parse(Base64Decode(base64code));
            return jData["textures"]["CAPE"]["url"].ToString();
        }

        private static string Base64Decode(string encodedString)
        {
            var bytes = Convert.FromBase64String(encodedString);
            var decodedString = Encoding.UTF8.GetString(bytes);
            return decodedString;
        }
    }
}
