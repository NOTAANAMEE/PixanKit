using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.PlayerModule.Player
{
    public partial class PlayerBase
    {
        /// <inheritdoc/>
        public virtual void LoadFromJSON(JObject jData)
        {
            _uid = (jData["uid"] ?? "").ToString();
            _name = (jData["name"] ?? "").ToString();
            _accesstoken = (jData["accesstoken"] ?? "").ToString();
        }

        /// <summary>
        /// Converts the player's data to a JSON object.
        /// </summary>
        /// <returns>A <see cref="JObject"/> representing the player's data.</returns>
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
    }
}
