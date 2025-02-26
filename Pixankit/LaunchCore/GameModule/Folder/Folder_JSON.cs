using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule
{
    public partial class Folder
    {
        /// <inheritdoc/>
        public void LoadFromJSON(JObject obj)
        {
            _folderpath = obj.GetOrDefault(JSON.Format.ToString, "path", "");
            Alias = obj.GetOrDefault(JSON.Format.ToString, "alias", ""); ;
        }

        /// <inheritdoc/>
        public JObject ToJSON()
        {
            return new JObject()
            {
                { "path" , _folderpath },
                { "alias" , Alias },
            };
        }
    }
}
