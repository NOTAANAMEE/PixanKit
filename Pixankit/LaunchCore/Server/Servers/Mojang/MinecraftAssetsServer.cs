using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Server.Servers.Mojang;

/// <summary>
/// Assets Server Abstraction Download Assets From The Server
/// </summary>
public class MinecraftAssetsServer : ResourceServer
{
    /// <summary>
    /// Initor With 1 Default Official Server
    /// </summary>
    public MinecraftAssetsServer()
    {
        Mirrors = [new("", "https://resources.download.minecraft.net")];
        Current = Mirrors[0];
    }

    /// <summary>
    /// Get The URL Of The Asset
    /// </summary>
    /// <param name="hash">{"hash":"This is the hash parameter", "size":8964}</param>
    /// <returns>The URL</returns>
    public string GetAssetsUrl(string hash)
    {
        var oriurl = $"https://resources.download.minecraft.net/{hash[0..2]}/{hash}";
        return Current?.Replace(oriurl) ?? oriurl;
    }

    /// <summary>
    /// Get The URL Of The Assets
    /// </summary>
    /// <param name="jData"></param>
    /// <returns>The URL Of The Asset</returns>
    public string GetAssetsUrl(JObject jData)
    {
        var hash = jData.First?["hash"]?.ToString() ?? throw new Exception();
        return Current?.Replace(
                   $"https://resources.download.minecraft.net/{hash[0..2]}/{hash}") ??
               throw new Exception();
    }

    /// <summary>
    /// Get The Target Location Of The Asset File
    /// </summary>
    /// <param name="jData"></param>
    /// <returns></returns>
    public string GetFileLocation(JObject jData)
    {
        var hash = jData.First?["hash"]?.ToString() ?? throw new Exception();
        return $"/{hash[0..2]}/{hash}";
    }
}