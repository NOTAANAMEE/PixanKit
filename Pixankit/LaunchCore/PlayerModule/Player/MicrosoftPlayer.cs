using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.PlayerModule.MojangAPI;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
    /// <summary>
    /// Represents a Microsoft account player in the Minecraft environment.
    /// </summary>
    public class MicrosoftPlayer:PlayerBase
    {
        /// <summary>
        /// Gets the latest login time. Re-login is required after 1 day.
        /// </summary>
        public DateTime LatestLoginTime { get => _latestLoginTime; }

        /// <summary>
        /// Gets the URL of the player's skin. Different images correspond to different URLs.
        /// </summary>
        public string SkinURL { get => _skinURL; }

        /// <summary>
        /// Gets the URL of the player's cape.
        /// </summary>
        public string CapeURL { get => _capeURL; }

        /// <summary>
        /// Gets the local cache path of the skin. It automatically updates if the <see cref="SkinURL"/> changes.
        /// </summary>
        public string SkinCachePath
        {
            get => Files.SkinCacheDir + $"/{UID}-skin.png";
        }

        private DateTime _latestLoginTime;

        private string _skinURL = "";

        private string _capeURL = "";

        private string refreshtoken = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftPlayer"/> class using JSON data.
        /// </summary>
        /// <param name="jData">The JSON data representing the player.</param>
        public MicrosoftPlayer(JObject jData):base(jData)
        {
            _latestLoginTime = 
                jData.GetValue(Format.ToDateTime, "logintime");
            _skinURL = 
                jData.GetValue(Format.ToString, "sinurl");
            _capeURL = 
                jData.GetValue(Format.ToString, "capeurl");
            refreshtoken =
                jData.GetValue(Format.ToString, "refreshtoken");
            _type = PlayerType.microsoft;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftPlayer"/> class for internal use.
        /// </summary>
        protected MicrosoftPlayer() { }

        /// <summary>
        /// Logs in a player using a login code.
        /// </summary>
        /// <param name="loginCode">The login code for Microsoft authentication.</param>
        /// <returns>A task representing the asynchronous operation, with a <see cref="MicrosoftPlayer"/> as the result.</returns>
        public static async Task<MicrosoftPlayer> Login(string loginCode)
        {
            var ret1 = await MojangLogin.GetMSToken(loginCode);
            var ret2 = await MojangLogin.XBoxAuthorize(ret1.MSaccessToken);
            var ret3 = await MojangLogin.XSTSVerification(ret2.Xboxtoken);
            var ret4 = await MojangLogin.MinecraftAccessToken(ret3);
            var ret5 = await MojangLogin.MinecraftUid(ret4);
            MicrosoftPlayer player = new()
            {
                _name = ret5.Name,
                _uid = ret5.Uid,
                _skinURL = ret5.SkinUrl,
                _accesstoken = ret4,
                refreshtoken = ret1.MSrefreshToken,
                _latestLoginTime = DateTime.Now,
                _type = PlayerType.microsoft,
            };
            player._capeURL = await MojangSkin.GetCapeURL(player);
            return player;
        }

        /// <summary>
        /// Re-logs in the player using the refresh token.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ReLogin() 
        {
            TimeSpan span = DateTime.Now - LatestLoginTime;
            if (span.Days >= 1)
            {
                var ret1 = await MojangLogin.RefreshMSToken(refreshtoken);
                var ret2 = await MojangLogin.XBoxAuthorize(ret1.MSaccessToken);
                var ret3 = await MojangLogin.XSTSVerification(ret2.Xboxtoken);
                _accesstoken = await MojangLogin.MinecraftAccessToken(ret3);
                refreshtoken = ret1.MSrefreshToken;
            }
            var ret5 = await MojangLogin.MinecraftUid(_accesstoken);
            _name = ret5.Name;
            _uid = ret5.Uid;
            if (_skinURL == ret5.SkinUrl && File.Exists(SkinCachePath)) return;
            _skinURL = ret5.SkinUrl;
            await RefreshSkinCache();
        }

        /// <summary>
        /// Refreshes the local skin cache.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshSkinCache()
        {
            HttpClient client = new();
            var response = await client.GetStreamAsync(this._skinURL);
            FileStream fs = new(
                SkinCachePath, FileMode.Create);
            response.CopyTo(fs);
            fs.Close();
            response.Dispose();
        }

        /// <summary>
        /// Converts the player's data to a JSON object.
        /// </summary>
        /// <returns>A <see cref="JObject"/> representing the player's data.</returns>
        public override JObject ToJSON()
        {
            JObject jobj = base.ToJSON();
            jobj.Add("logintime", _latestLoginTime);
            jobj.Add("skinurl", _skinURL);
            jobj.Add("capeurl", _capeURL);
            jobj.Add("refreshtoken", refreshtoken);
            return jobj;
        }
    }
}
