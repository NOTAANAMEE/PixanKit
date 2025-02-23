using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    item.Key.GameFolderPath,
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
