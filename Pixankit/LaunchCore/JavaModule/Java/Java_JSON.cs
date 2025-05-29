using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.JavaModule.Java;

public partial class JavaRuntime
{
    /// <inheritdoc/>
    public void LoadFromJson(JObject obj)
    {
        _javaFolder = obj.GetOrDefault(Format.ToString, "path", "");
        _version = obj.GetOrDefault((a) => (ushort)a, "version", (ushort)0);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public JObject ToJson()
    {
        return new JObject()
        {
            { "path", _javaFolder },
            { "version", _version },
        };
    }
}