using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
    public class OfflinePlayer:PlayerBase
    {
        public override string UID 
        { 
            get => "00000FFFFFFFFFFFFFFFFFFFFFF1FF43"; 
        }

        public override string AccessToken
        {
            get => UID;
        }

        public OfflinePlayer(string name): base(null)
        {
            _name = name;
            _uid = $"uid{name}";
            _accesstoken = "";
            _type = PlayerType.offline;
        }

        public OfflinePlayer(JObject jData) : base(jData)
        {
            _type = PlayerType.offline;
        }
    }
}
