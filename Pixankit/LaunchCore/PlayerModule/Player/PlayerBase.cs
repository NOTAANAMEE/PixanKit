using PixanKit.LaunchCore.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
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

        public PlayerType LoginType
        {
            get => _type;
        }

        protected string _uid = "";

        protected string _name = "";

        protected PlayerType _type;

        protected string _accesstoken = "";

        protected string refreshToken = "";

        protected DateTime lastLogin;

        public PlayerBase(JObject? jData)
        {
            if (jData == null) return;
            _uid = jData["uid"].ToString();
            _name = jData["name"].ToString();
            _accesstoken = jData["accesstoken"].ToString();
        }

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

        protected static bool SamePlayer(PlayerBase? player1, PlayerBase? player2)
        {
            return player1._uid == player2._uid;
        }

        public static bool operator ==(PlayerBase? player1, PlayerBase? player2)
        {
            if (player1 is null)return player2 is null;
            else if (player2 is null) return false;
            return SamePlayer(player1, player2);
        }

        public static bool operator !=(PlayerBase? player1, PlayerBase? player2)
        {
            return !(player1 == player2);
        }

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

    public enum PlayerType 
    {
        microsoft,
        offline,
        yggdrasil
    }
}
