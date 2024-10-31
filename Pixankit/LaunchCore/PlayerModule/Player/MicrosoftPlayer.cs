using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.PlayerModule.MojangAPI.Login;
using PixanKit.LaunchCore.PlayerModule.MojangAPI.Skin;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
    /// <summary>
    /// Microsoft Login Player
    /// </summary>
    public class MicrosoftPlayer:PlayerBase
    {
        /// <summary>
        /// The Login Time. Need To Relogin After 1 Day 
        /// </summary>
        public DateTime LatestLoginTime { get => _latestLoginTime; }

        /// <summary>
        /// The URL Of The Skin. Different Images Will Be Different URLs
        /// </summary>
        public string SkinURL { get => _skinURL; }

        /// <summary>
        /// The URL Of The Cape.
        /// </summary>
        public string CapeURL { get => _capeURL; }

        /// <summary>
        /// Local Cache Of The Skin. It Will Automatically Change If The <see cref="SkinURL"/> Changes
        /// </summary>
        public string SkinCachePath
        {
            get => Files.SkinCache + $"/{UID}-skin.png";
        }

        private DateTime _latestLoginTime;

        private string _skinURL = "";

        private string _capeURL = "";

        private string refreshtoken = "";

        /// <summary>
        /// Init A Microsoft Player According To jData
        /// </summary>
        /// <param name="jData">
        /// {
        ///     "uid":${uid},
        ///     "accesstoken":${accesstoken},
        ///     "name":${name},
        ///     "logintime":${logintime}
        /// }
        /// </param>
        public MicrosoftPlayer(JObject jData):base(jData)
        {
            _latestLoginTime = DateTime.Parse(jData["logintime"].ToString());
            _skinURL = jData["skinurl"].ToString();
            _capeURL = jData["capeurl"].ToString();
            refreshtoken = jData["refreshtoken"].ToString();
            _type = PlayerType.microsoft;
            if (DateTime.Now - _latestLoginTime >= TimeSpan.FromDays(1))
            {
                _ = ReLogin();
                _latestLoginTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Init
        /// </summary>
        protected MicrosoftPlayer() { }

        /// <summary>
        /// Init a player by giving the login code
        /// </summary>
        /// <param name="loginCode"></param>
        /// <returns></returns>
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
        /// Relogin the player by refreshtoken
        /// </summary>
        /// <returns></returns>
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
            if (_skinURL == ret5.SkinUrl && File.Exists(Localize.PathLocalize(SkinCachePath))) return;
            _skinURL = ret5.SkinUrl;
            await RefreshSkinCache();
        }

        /// <summary>
        /// This method refresh the local skin cache
        /// </summary>
        public async Task RefreshSkinCache()
        {
            HttpClient client = new();
            var response = await client.GetStreamAsync(this._skinURL);
            FileStream fs = new(
                Localize.PathLocalize(SkinCachePath), FileMode.Create);
            response.CopyTo(fs);
            fs.Close();
            response.Dispose();
        }

        /// <summary>
        /// To JSON Object
        /// </summary>
        /// <returns></returns>
        public override JObject ToJSON()
        {
            JObject jobj = base.ToJSON();
            jobj.Add("logintime", _latestLoginTime.ToString());
            jobj.Add("skinurl", _skinURL);
            jobj.Add("capeurl", _capeURL);
            jobj.Add("refreshtoken", refreshtoken);
            return jobj;
        }
    }
}
