using Newtonsoft.Json.Linq;

namespace PixanKit.ModController.Module
{
    public partial class ModCollection
    {
        /// <inheritdoc/>
        public void LoadFromJSON(JObject cache)
        {
            ModCache = cache;
        }

        ///<inheritdoc/>
        public JObject ToJSON()
        {
            JObject jsonData = [];
            foreach (var item in ModFiles)
            {
                if (item.Value.ValidStructure)
                    jsonData.Add(item.Key, item.Value.ToJSON());
                else
                    jsonData.Add(item.Value.FileName, item.Value.ToJSON());
            }
            return jsonData;
        }
    }
}
