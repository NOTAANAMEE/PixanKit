using PixanKit.LaunchCore.Server.Servers.Microsoft;
using PixanKit.LaunchCore.Server.Servers.ModLoader;
using PixanKit.LaunchCore.Server.Servers.Mojang;

namespace PixanKit.LaunchCore.Server;

/// <summary>
/// Collection Of Servers
/// </summary>
public static class ServerList
{
    /// <summary>
    /// Server Instance For Microsoft Account
    /// </summary>
    public static MsLoginServer MicrosoftLoginServer = new();

    /// <summary>
    /// Server Instance For XBox
    /// </summary>
    public static XboxServer XboxLoginServer = new();

    /// <summary>
    /// Server Instance For XSTS
    /// </summary>
    public static XstsServer XstsServer = new();

    /// <summary>
    /// Server Instance For Mojang Account
    /// </summary>
    public static MojangLoginServer MojangLoginServer = new();

    /// <summary>
    /// Server Instance For Minecraft Versions
    /// </summary>
    public static MinecraftVersionServer MinecraftVersionServer = new();

    /// <summary>
    /// Server Instance For Minecraft Assets
    /// </summary>
    public static MinecraftAssetsServer MinecraftAssetsServer = new();

    /// <summary>
    /// The list of <see cref="ModLoaderServer"/>. The keys are the name of the mod loaders
    /// </summary>
    public static Dictionary<string, ModLoaderServer> ModLoaderServers = [];
}