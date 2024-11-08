using PixanKit.LaunchCore.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.Log;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
    /// <summary>
    /// Player Base. Base Class For Players
    /// </summary>
    public class PlayerBase:IToJSON
    {
        /// <summary>
        /// Player Name
        /// Necessary For Launch
        /// </summary>
        public string Name
        {
            get => _name;
        }

        /// <summary>
        /// Player UID
        /// Necessary For Launch
        /// </summary>
        public virtual string UID
        {
            get => _uid;
        }

        /// <summary>
        /// Player Access Token To Verification
        /// Necessary For Launch
        /// </summary>
        public virtual string AccessToken 
        {
            get => _accesstoken;
        }

        /// <summary>
        /// Login Type
        /// </summary>
        public PlayerType LoginType
        {
            get => _type;
        }

        /// <summary>
        /// Player ID
        /// </summary>
        protected string _uid = "";

        /// <summary>
        /// Player Name
        /// </summary>
        protected string _name = "";

        /// <summary>
        /// Player Login Type
        /// </summary>
        protected PlayerType _type;

        /// <summary>
        /// Player Mojang AccessToken
        /// </summary>
        protected string _accesstoken = "";

        /// <summary>
        /// Player Microsoft Refresh Token
        /// </summary>
        protected string refreshToken = "";

        /// <summary>
        /// Last Login Time
        /// </summary>
        protected DateTime lastLogin;

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="jData">jData Of A Player</param>
        public PlayerBase(JObject? jData)
        {
            if (jData == null) return;
            _uid = (jData["uid"]?? "").ToString();
            _name = (jData["name"] ?? "").ToString();
            _accesstoken = (jData["accesstoken"] ?? "").ToString();
            Logger.Info($"Player Init, Name:{Name} UID:{UID}");
        }

        /// <summary>
        /// Initor
        /// </summary>
        protected PlayerBase() { }

        /// <summary>
        /// Inline Player Data
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public string InlinePlayer(string arg)
        {
            arg = arg.Replace("${auth_player_name}", Name);
            arg = arg.Replace("${auth_uuid}", UID);
            arg = arg.Replace("${auth_access_token}", AccessToken);
            arg = arg.Replace("${user_type}", "msa");
            return arg;
        }

        /// <summary>
        /// Test Whether The Players Are The Same
        /// </summary>
        /// <param name="player1">Player1</param>
        /// <param name="player2">Player2</param>
        /// <returns>bool</returns>
        protected static bool SamePlayer(PlayerBase player1, PlayerBase player2)
        {
            return player1._uid == player2._uid;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="player1"><inheritdoc/></param>
        /// <param name="player2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(PlayerBase? player1, PlayerBase? player2)
        {
            if (player1 is null)return player2 is null;
            else if (player2 is null) return false;
            return SamePlayer(player1, player2);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="player1"><inheritdoc/></param>
        /// <param name="player2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(PlayerBase? player1, PlayerBase? player2)
        {
            return !(player1 == player2);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public virtual JObject ToJSON()
        {
            JObject jobj = new()
            {
                { "uid", _uid },
                { "name", _name },
                { "accesstoken", _accesstoken },
                { "type", _type.ToString()}
            };
            return jobj;
        }

        /// <summary>
        /// I hate Warnings so I wrote this
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _uid.GetHashCode();
        }

        /// <summary>
        /// I hate Warnings so I wrote this
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;
            return ((PlayerBase)obj) == this;
        }
    }

    /// <summary>
    /// Player Login Type
    /// </summary>
    public enum PlayerType 
    {
        /// <summary>
        /// Microsoft Login
        /// </summary>
        microsoft,
        /// <summary>
        /// Offline Login
        /// </summary>
        offline,
        /// <summary>
        /// Third-Party Yggdrasil Server
        /// </summary>
        yggdrasil
    }
}
