using Newtonsoft.Json.Linq;

namespace PixanKit.ModController.Module
{
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
                if (item.Value.ValidStructure)
                    jsonData.Add(item.Key, item.Value.ToJson());
                else
                    jsonData.Add(item.Value.FileName, item.Value.ToJson());
            }
            return jsonData;
        }
    }
}
