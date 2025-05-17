using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.ModLoader;

/// <summary>
/// Represents an abstract base class for a mod loader mirror server.
/// </summary>
public abstract class ModLoaderMirror : MirrorServer
{
    #region CancellableMethod
    /// <summary>
    /// Checks whether the builds exist for a specific Minecraft version.
    /// </summary>
    /// <param name="mcversion">The Minecraft version to check builds for.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the builds exist.</returns>
    public abstract Task<bool> CheckBuild(string mcversion, CancellationToken token);

    /// <summary>
    /// Retrieves the build list for a specific Minecraft version.
    /// </summary>
    /// <param name="mcversion">The Minecraft version to retrieve builds for.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a JSON array of mod loader versions.</returns>
    public abstract Task<JArray> GetBuild(string mcversion, CancellationToken token);

    /// <summary>
    /// Retrieves the URL of the mod loader installer.
    /// </summary>
    /// <param name="modloaderinf">A JSON object containing mod loader information.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the URL of the mod loader installer.</returns>
    public abstract Task<string> GetUrl(JObject modloaderinf, CancellationToken token);
    #endregion
}