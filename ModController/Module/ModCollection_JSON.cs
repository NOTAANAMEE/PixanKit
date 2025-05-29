using Newtonsoft.Json.Linq;

namespace PixanKit.ModController.Module;

public partial class ModCollection
{
    /// <inheritdoc/>
    public void LoadFromJson(JObject cache)
    {
        ModCache = cache;
    }

    ///<inheritdoc/>
    public JObject ToJson()
    {
        JObject jsonData = [];
        foreach (var item in ModFiles)
        {
            jsonData.Add(item.Value.ValidStructure ? 
                item.Key : item.Value.FileName, 
                item.Value.ToJson());
        }
        return jsonData;
    }
}