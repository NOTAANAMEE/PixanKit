using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.ModController.Module;

public partial class ModModule
{
    /// <inheritdoc/>
    public void LoadFromJson(JObject obj)
    {
        OpenContent(obj);
    }

    /// <inheritdoc/>
    public JObject ToJson()
    {
        JObject moddedGamedata = [];
        foreach (var item in ModdedGames)
            moddedGamedata.Add(
                Json.PathToKey(item.Key.GameFolderPath),
                item.Value.ToJson());

        JArray modMetadata = [];
        foreach (var item in ModData)
            modMetadata.Add(item.Value.ToJson());

        return new()
        {
            { "metadata", modMetadata },
            { "games" , moddedGamedata }
        };

    }
}