using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Exceptions;

/// <summary>
/// Represents the exception that some keys might not contains in the JSON document
/// </summary>
/// <param name="token">the token that the value is from</param>
/// <param name="key">the expected key/path of the value</param>
/// <param name="message">give some hint?</param>
public class JsonKeyException(JToken token, string key, string message) : Exception(message)
{
    /// <summary>
    /// Represents the root token to find the value
    /// </summary>
    public readonly JToken Token = token;

    /// <summary>
    /// Represents the path to find the value
    /// </summary>
    public readonly string Key = key;
}