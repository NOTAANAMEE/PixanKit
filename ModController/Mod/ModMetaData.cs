using PixanKit.ModController.Module;
using PixanKit.ModController.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.LibraryData;

namespace PixanKit.ModController.Mod
{
    public class ModMetaData()
    {
        public string[] Authors { get; internal set; } = [];

        public string ModID { get; set; } = "";

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public Dictionary<string, List<ModFile>> ModFiles { get; private set; } = [];

        public string ImageCache { get; set; } = "";

        public int ReferenceTime { get => ModFiles.Sum(a => a.Value.Count); }

        public Dictionary<string, string> NewestVersions { get; private set; } = [];

        public void Register(ModFile modFile)
        {
            string mcversion;
            modFile.MetaData = this;
            mcversion = modFile.Owner?.Owner?.Version ??
                throw new NullReferenceException();
            if (!ModFiles.TryGetValue(mcversion, out var list))
                ModFiles.Add(mcversion, [modFile]);
            else list.Add(modFile);
        }

        public async Task GetUpdate(CancellationToken token)
        {
            if (ModModule.Instance?.ModVersionGetter == null)
                throw new InvalidOperationException();
            var jarray = await ModModule.Instance.ModVersionGetter.
                GetVersionsAsync(token);
            ReadUpdate(jarray);
        }

        private void ReadUpdate(JArray versions)
        {
            var tmpdata = new Dictionary<string, List<ModFile>>(ModFiles);
            NewestVersions = [];
            foreach (var modversion in versions)
            {
                string mcversion = modversion["game_versions"]?[0]?.ToString() ?? "";
                if (tmpdata.ContainsKey(mcversion))
                {
                    NewestVersions.Add(mcversion, modversion["version_number"]?.ToString() ?? "");
                    tmpdata.Remove(mcversion);
                }
                if (tmpdata.Count == 0) return;
            }
        }

        public JObject ToJSON()
        {
            return new()
            {
                { "id", ModID
                },
                { "name", Name
                },
                { "description", Description
                },
                { "icon", ImageCache
                },
                { "authors", JArray.FromObject(Authors)
                }
            };
        }
    }
}
