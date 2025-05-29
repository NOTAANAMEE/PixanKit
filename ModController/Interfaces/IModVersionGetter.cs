using Newtonsoft.Json.Linq;

namespace PixanKit.ModController.Interfaces;

/// <summary>
/// This interface is used to get the version list of mods
/// </summary>
public interface IModVersionGetter
{
    /// <summary>
    /// Gets the list of versions of the mod.
    /// </summary>
    /// <param name="modId">The ID of the mod</param>
    /// <param name="token">Cancel or not</param>
    /// <returns>The array of files.</returns>
    public Task<JArray> GetVersionsAsync(string modId, CancellationToken token);
}