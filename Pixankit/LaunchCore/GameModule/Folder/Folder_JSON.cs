using Newtonsoft.Json.Linq;
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
            _folderpath = obj["path"]?.ToString() ?? "";
            Alias = obj["alias"]?.ToString() ?? "";
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
