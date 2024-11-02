using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
    /// <summary>
    /// Offline Player. Player Might Not Purchase Minecraft :(<br/>
    /// Please Decide Whether To Open Offline Channels To Players According To Local Laws.
    /// </summary>
    public class OfflinePlayer:PlayerBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string UID 
        { 
            get => "00000FFFFFFFFFFFFFFFFFFFFFF1FF43"; 
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string AccessToken
        {
            get => UID;
        }

        /// <summary>
        /// Initor With Name
        /// </summary>
        /// <param name="name">Player Name</param>
        public OfflinePlayer(string name): base(null)
        {
            _name = name;
            _uid = $"uid{name}offline";
            _accesstoken = "";
            _type = PlayerType.offline;
        }

        /// <summary>
        /// Initor With JSON Data
        /// </summary>
        /// <param name="jData"></param>
        public OfflinePlayer(JObject jData) : base(jData)
        {
            _type = PlayerType.offline;
        }
    }
}
