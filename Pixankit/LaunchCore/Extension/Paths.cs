using System.Text.RegularExpressions;

namespace PixanKit.LaunchCore.Extension;

/// <summary>
/// Path Dictionary
/// </summary>
public static partial class Paths
{
    private static readonly Dictionary<string, string> PathDict = [];

    private static readonly Lock Locker = new();


    /// <summary>
    /// Add A New Path
    /// </summary>
    /// <param name="key">Key For Finding And Replacing</param>
    /// <param name="value">The Actual Path</param>
    private static void Add(string key, string value)
    {
        lock (Locker)
            PathDict.Add(key, value);
    }

    /// <summary>
    /// Set The Path
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value">The Final Path</param>
    private static void Set(string key, string value)
    {
        lock (Locker) PathDict[key] = value;
    }

    /// <summary>
    /// If it Has Key, call Set() else call Add()
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void TrySet(string key, string value)
    {
        lock (Locker)
        {
            if (!PathDict.ContainsKey(key)) Add(key, value);
            else Set(key, value);
        }
    }

    /// <summary>
    /// Try to get the value. If exists, value will be he path. Else, value
    /// will be null and return false
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetValue(string key, out string? value)
    {
        value = null;
        if (!PathDict.TryGetValue(key, out var ret))
        {
            return false;
        }
        value = Replace(ret);
        return true;
    }

    /// <summary>
    /// Get The Processed Path
    /// </summary>
    /// <param name="key"></param>
    /// <returns>THe Final Path After Replacement</returns>
    public static string Get(string key)
        => Replace(PathDict[key]);

    /// <summary>
    /// Get the Path. If not exist, add the item.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetOrAdd(string key, string value)
    {
        if (!PathDict.TryGetValue(key, out var ret))
        {
            Add(key, value);
            ret = value;
        }
        return Replace(ret);
    }


    private static string Replace(string value)
    {
        var result = MyRegex().Replace(value, match =>
        {
            var key = match.Groups[1].Value;
            return PathDict.TryGetValue(key, out var s) ? s : match.Value;
        });
        return result;
    }

    [GeneratedRegex(@"\${(.*?)}")]
    private static partial Regex MyRegex();
}