using Newtonsoft.Json.Linq;
using PixanKit.ModController.Module;

namespace PixanKit.ModController.Mod;

/// <summary>
/// Represents metadata for a mod.
/// </summary>
public class ModMetaData
{
    /// <summary>
    /// Gets or sets the list of authors of the mod.
    /// </summary>
    public string[] Authors { get; internal set; } = [];

    /// <summary>
    /// Gets or sets the unique identifier of the mod.
    /// </summary>
    public string ModId { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the name of the mod.
    /// </summary>
    public string Name { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the description of the mod.
    /// </summary>
    public string Description { get; set; } = "Unknown";

    /// <summary>
    /// Gets the dictionary of mod files, where the key is the Minecraft version and the value is a list of mod files for that version.
    /// </summary>
    public Dictionary<string, List<ModFile>> ModFiles { get; private set; } = [];

    /// <summary>
    /// The lock object for synchronizing access to <see cref="ModFiles"/>.
    /// </summary>
    public readonly object ModFilesLocker = new();

    /// <summary>
    /// Gets or sets the cached path of the mod icon.
    /// </summary>
    public string ImageCache { get; set; } = "";

    /// <summary>
    /// Gets the total number of mod files across all versions.
    /// </summary>
    public int ReferenceTime => ModFiles.Sum(a => a.Value.Count);

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
        modFile.MetaData = this;
        var ownerVersion = modFile.Owner?.Owner.Version ??
                        throw new NullReferenceException();
        lock (ModFilesLocker)
        {
            if (!ModFiles.TryGetValue(ownerVersion, out var list))
                ModFiles.Add(ownerVersion, [modFile]);
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
            throw new InvalidOperationException(
                "Please implement IModVersionGetter before using this method");
        var jArray = await ModModule.Instance.ModVersionGetter.
            GetVersionsAsync(ModId, token);
        ReadUpdate(jArray);
    }

    /// <summary>
    /// Parses and updates the latest mod version information.
    /// </summary>
    /// <param name="versions">The JSON data containing mod versions retrieved from a remote API.</param>
    private void ReadUpdate(JArray versions)
    {
        Dictionary<string, List<ModFile>> dictionary = new(ModFiles);
        NewestVersions = [];
        foreach (var modVerToken in versions)
        {
            var mcVersion = modVerToken["game_versions"]?[0]?.ToString() ?? "";
            if (dictionary.ContainsKey(mcVersion))
            {
                NewestVersions.Add(mcVersion, modVerToken["version_number"]?.ToString() ?? "");
                dictionary.Remove(mcVersion);
            }
            if (dictionary.Count == 0) return;
        }
    }

    /// <summary>
    /// Converts the mod metadata to JSON format.
    /// </summary>
    /// <returns>A <see cref="JObject"/> containing the mod metadata.</returns>
    public JObject ToJson()
    {
        return new()
        {
            { "id", ModId },
            { "name", Name },
            { "description", Description },
            { "icon", ImageCache },
            { "authors", JArray.FromObject(Authors) }
        };
    }
}