using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule
{
    public partial class Folder
    {
        /// <inheritdoc/>
        public void LoadFromJSON(JObject obj)
        {
            _folderpath = obj.GetOrDefault(Format.ToString, "path", "");
            Alias = obj.GetOrDefault(Format.ToString, "alias", ""); ;
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
