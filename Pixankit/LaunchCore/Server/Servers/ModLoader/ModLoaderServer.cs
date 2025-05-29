using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.ModLoader;

/// <summary>
/// Server Layer For Mod Loader Installer
/// </summary>
public abstract class ModLoaderServer : ResourceServer
{


    /// <summary>
    /// Current Server
    /// </summary>
    public new ModLoaderMirror Current
    {
        get => (base.Current ?? this.Mirrors[0]) as ModLoaderMirror ?? throw new Exception();
        set => base.Current = value;
    }

    /// <summary>
    /// The Name Of The Mo Loader
    /// </summary>
    public string Name = "";

    /// <summary>
    /// Init A Mod Loader Server
    /// </summary>
    public ModLoaderServer(string name)
    {
        Name = name;
        ServerList.ModLoaderServers.Add(name, this);
    }

    /// <summary>
    /// Check Whether Minecraft Version Is Supported
    /// </summary>
    /// <param name="minecraftversion"></param>
    /// <returns></returns>
    public Task<bool> CheckBuild(string minecraftversion)
        => CheckBuild(minecraftversion, CancellationToken.None);

    /// <summary>
    /// Check Whether Minecraft Version Is Supported
    /// </summary>
    /// <param name="cancellationToken">Cancel Token</param>
    /// <param name="minecraftversion"></param>
    /// <returns></returns>
    public Task<bool> CheckBuild(string minecraftversion, CancellationToken cancellationToken)
        => Current.CheckBuild(minecraftversion, cancellationToken);

    /// <summary>
    /// Get The URL Of The Mod Loader Installer
    /// </summary>
    /// <param name="modloaderinf"></param>
    /// <returns></returns>
    public Task<string> GetUrl(JObject modloaderinf)
        => GetUrl(modloaderinf, CancellationToken.None);

    /// <summary>
    /// Get the URL of the mod loader installer.
    /// This is a cancellable task
    /// </summary>
    /// <param name="modloaderinf">the modloader JSON data</param>
    /// <param name="token">token</param>
    /// <returns>the specific url of the installer</returns>
    public Task<string> GetUrl(JObject modloaderinf, CancellationToken token)
        => Current.GetUrl(modloaderinf, token);

    /// <summary>
    /// Get The Modloader Versions That Are Suitable For The Minecraft Version
    /// </summary>
    /// <param name="minecraftversion">Minecraft Version</param>
    /// <returns>List Of The ModLoader Versions</returns>
    public Task<JArray> GetVersionsForMinecraft(string minecraftversion)
        => GetVersionsForMinecraft(minecraftversion, CancellationToken.None);

    /// <summary>
    /// Get suitable versions for supported Minecraft version
    /// </summary>
    /// <param name="minecraftversion">the version of Minecraft</param>
    /// <param name="token">the cancellation token</param>
    /// <returns>the list that contains the information of the modloaders</returns>
    public Task<JArray> GetVersionsForMinecraft(string minecraftversion, CancellationToken token)
        => Current.GetBuild(minecraftversion, token);
}