using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Json;

/// <summary>
/// Defines a method to convert an object to a JSON representation.
/// </summary>
public interface IToJson
{
    /// <summary>
    /// Load the data from a JSON object
    /// </summary>
    /// <param name="obj">The JSON object</param>
    public void LoadFromJson(JObject obj);

    /// <summary>
    /// Converts the implementing object to a JSON object.
    /// </summary>
    /// <returns>A <see cref="JObject"/> representing the object's data.</returns>
    public JObject ToJson();
}