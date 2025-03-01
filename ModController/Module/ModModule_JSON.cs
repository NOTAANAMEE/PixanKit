using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.ModController.Module
{
    public partial class ModModule
    {
        /// <inheritdoc/>
        public void LoadFromJSON(JObject obj)
        {
            OpenContent(obj);
        }

        /// <inheritdoc/>
        public JObject ToJSON()
        {
            JObject moddedGamedata = [];
            foreach (var item in ModdedGames)
                moddedGamedata.Add(
                    JSON.PathToKey(item.Key.GameFolderPath),
                    item.Value.ToJSON());

            JArray modMetadata = [];
            foreach (var item in ModDatas)
                modMetadata.Add(item.Value.ToJSON());

            return new()
            {
                { "metadata", modMetadata },
                { "games" , moddedGamedata }
            };

        }
    }
}
