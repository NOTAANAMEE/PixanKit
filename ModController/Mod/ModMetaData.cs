using Newtonsoft.Json.Linq;
using PixanKit.ModController.Module;

namespace PixanKit.ModController.Mod
{
    /// <summary>
    /// Represents metadata for a mod.
    /// </summary>
    public class ModMetaData()
    {
        /// <summary>
        /// Gets or sets the list of authors of the mod.
        /// </summary>
        public string[] Authors { get; internal set; } = [];

        /// <summary>
        /// Gets or sets the unique identifier of the mod.
        /// </summary>
        public string ModID { get; set; } = "Unkonwn";

        /// <summary>
        /// Gets or sets the name of the mod.
        /// </summary>
        public string Name { get; set; } = "Unkonwn";

        /// <summary>
        /// Gets or sets the description of the mod.
        /// </summary>
        public string Description { get; set; } = "Unkonwn";

        /// <summary>
        /// Gets the dictionary of mod files, where the key is the Minecraft version and the value is a list of mod files for that version.
        /// </summary>
        public Dictionary<string, List<ModFile>> ModFiles { get; private set; } = [];

        /// <summary>
        /// The lock object for synchronizing access to <see cref="ModFiles"/>.
        /// </summary>
        public readonly object ModFiles_Locker = new();

        /// <summary>
        /// Gets or sets the cached path of the mod icon.
        /// </summary>
        public string ImageCache { get; set; } = "";

        /// <summary>
        /// Gets the total number of mod files across all versions.
        /// </summary>
        public int ReferenceTime { get => ModFiles.Sum(a => a.Value.Count); }

        /// <summary>
        /// Gets the latest version information of the mod, where the key is the Minecraft version and the value is the latest mod version number.
        /// </summary>
        public Dictionary<string, string> NewestVersions { get; private set; } = [];

        /// <summary>
        /// Registers a mod file under the corresponding Minecraft version.
        /// </summary>
        /// <param name="modFile">The mod file to register.</param>
        /// <exception cref="NullReferenceException">Thrown if the Minecraft version cannot be retrieved.</exception>
        public void Register(ModFile modFile)
        {
            string mcversion;
            modFile.MetaData = this;
            mcversion = modFile.Owner?.Owner?.Version ??
                throw new NullReferenceException();
            lock (ModFiles_Locker)
            {
                if (!ModFiles.TryGetValue(mcversion, out var list))
                    ModFiles.Add(mcversion, [modFile]);
                else list.Add(modFile);
            }
        }

        /// <summary>
        /// Asynchronously retrieves the latest mod version information.
        /// </summary>
        /// <param name="token">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if ModVersionGetter is null.</exception>
        public async Task GetUpdate(CancellationToken token)
        {
            if (ModModule.Instance?.ModVersionGetter == null)
                throw new InvalidOperationException();
            var jarray = await ModModule.Instance.ModVersionGetter.
                GetVersionsAsync(ModID, token);
            ReadUpdate(jarray);
        }

        /// <summary>
        /// Parses and updates the latest mod version information.
        /// </summary>
        /// <param name="versions">The JSON data containing mod versions retrieved from a remote API.</param>
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

        /// <summary>
        /// Converts the mod metadata to JSON format.
        /// </summary>
        /// <returns>A <see cref="JObject"/> containing the mod metadata.</returns>
        public JObject ToJSON()
        {
            return new()
            {
                { "id", ModID },
                { "name", Name },
                { "description", Description },
                { "icon", ImageCache },
                { "authors", JArray.FromObject(Authors) }
            };
        }
    }
}
