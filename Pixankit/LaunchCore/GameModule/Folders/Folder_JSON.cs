using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.Folders;

public partial class Folder
{
    /// <inheritdoc/>
    public void LoadFromJson(JObject obj)
    {
        _folderPath = obj.GetOrDefault(Format.ToString, "path", "").Replace("\\", "/");
        Alias = obj.GetOrDefault(Format.ToString, "alias", ""); ;
    }

    /// <inheritdoc/>
    public JObject ToJson()
    {
        return new JObject()
        {
            { "path" , _folderPath },
            { "alias" , Alias },
        };
    }
}