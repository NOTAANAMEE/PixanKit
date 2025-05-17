using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Exceptions;

namespace PixanKit.LaunchCore.Server.Servers.Mojang;

/// <summary>
/// Mirror Servers For Minecraft Version Resources
/// </summary>
public class MinecraftVersionServer : ResourceServer
{
    private Cache? _cache;

    private record Cache(JArray Versions, DateTime UpdateTime);

    /// <summary>
    /// Init A MinecraftVersionServer
    /// </summary>
    public MinecraftVersionServer()
    {
        Mirrors = [new("", "https://piston-meta.mojang.com")];
        Current = Mirrors[0];
    }

    /// <summary>
    /// Get The Versions from the Mojang server.
    /// After called, the cache will be saved for 1 day
    /// </summary>
    /// <returns>The JArray that contains every version</returns>
    public async Task<JArray> GetVersionsAsync(CancellationToken token)
    {
        if (_cache == null || DateTime.Now - _cache.UpdateTime > TimeSpan.FromDays(1))
        {
            //UpdateIndex();
            await GetArrayFromNetwork(token);
        }
        if (_cache != null) return _cache.Versions;
        throw new Exception();
    }

    /// <summary>
    /// Get The Versions from the Mojang server.
    /// After called, the cache will be saved for 1 day
    /// </summary>
    /// <returns>The JArray that contains every version</returns>
    public JArray GetVersions()
    {
        var task = GetVersions(new CancellationToken());
        task.Wait();
        return task.Result;
    }

    /// <summary>
    /// Get the version from the Mojang Server
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<JArray> GetVersions(CancellationToken token)
    {
        if (_cache == null || DateTime.Now - _cache.UpdateTime > TimeSpan.FromDays(1))
        {
            await Update(token);
        }
        if (_cache != null) return _cache.Versions;
        throw new Exception();
    }

    /// <summary>
    /// Get the latest release from the JArray that contains all the versions
    /// </summary>
    /// <param name="jArray">The return from GetVersions</param>
    /// <returns>JObject from the JArray returned by GetVersions</returns>
    public JObject? GetLatestRelease(JArray jArray)
    {
        foreach (var token in jArray)
        {
            if (token["type"]?.ToString() == "release")
                return token as JObject;
        }
        return null;
    }

    /// <summary>
    /// Get the latest Snapshot from the JArray that contains all the versions
    /// </summary>
    /// <param name="jArray">The return from GetVersions</param>
    /// <returns>JObject from the JArray returned by GetVersions</returns>
    public JObject? GetLatestSnapshot(JArray jArray)
    {
        foreach (var token in jArray)
        {
            if (token["type"]?.ToString() != "release")
                return token as JObject;
        }
        return null;
    }

    /// <summary>
    /// Get the latest release minecraft from the server
    /// This is a cancellable task
    /// </summary>
    /// <param name="token"></param>
    /// <returns>The JSON data of the minecraft</returns>
    public async Task<JObject?> GetLatestRelease(CancellationToken token)
    {
        return GetLatestRelease(await GetVersions(token));
    }

    /// <summary>
    /// Get the latest snapshot minecraft from the server
    /// This is a cancellable task
    /// </summary>
    /// <param name="token"></param>
    /// <returns>The JSON data of the minecraft</returns>
    public async Task<JObject?> GetLatestSnapshot(CancellationToken token)
    {
        return GetLatestSnapshot(await GetVersions(token));
    }

    /// <summary>
    /// Get the URL of Json for Specific Minecraft Version
    /// </summary>
    /// <param name="jObject">The JObject from the Array returned by GetVersions
    /// </param>
    /// <returns>The URL for Json</returns>
    public string GetJsonUrl(JObject jObject)
    {
        return Replace(jObject["url"]?.ToString() ??
                       throw new JsonKeyException(jObject, "url",
                           "Minecraft JSON Document"));
    }

    /// <summary>
    /// Get the URL of Minecraft Jar File
    /// </summary>
    /// <param name="jObject">The Json Object parsed from the Json file for 
    /// the specific Minecraft Version</param>
    /// <returns>The URL</returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetMinecraftJarUrl(JObject jObject)
    {
        return Replace(jObject["downloads"]?["client"]?["url"]?.ToString() ??
                       throw new JsonKeyException(jObject, "downloads/client/url",
                           "Minecraft JSON Document"));
    }

    /// <summary>
    /// Get The URL Of Json For Assets
    /// </summary>
    /// <param name="jObject">Official Json For Specific Version</param>
    /// <returns>The URL</returns>
    public string GetAssetsJsonUrl(JObject jObject)
    {
        return Replace(jObject["assetIndex"]?["url"]?.ToString() ??
                       throw new JsonKeyException(jObject, "/assetIndex/url", "Version JSON document"));
    }

    /// <summary>
    /// Update The Cache
    /// </summary>
    public async Task Update(CancellationToken token)
    {
        await GetArrayFromNetwork(token);
    }

    private async Task GetArrayFromNetwork(CancellationToken token)
    {
        HttpClient client = new();
        var ret = await client.GetAsync(
            Replace("https://piston-meta.mojang.com/mc/game/version_manifest.json"), token);
        var tmp = await ret.Content.ReadAsStringAsync(token);
        var jObj = JObject.Parse(tmp);
        _cache = new Cache(jObj["versions"] is JArray array ? array : [], DateTime.Now);
    }
}