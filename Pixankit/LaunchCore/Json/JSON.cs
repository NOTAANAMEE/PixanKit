using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.Json;

/// <summary>
/// This class implements some methods that helps read the json
/// and merge the json
/// </summary>
public static class Json
{
    /// <summary>
    /// The method reads the JSON data from file
    /// </summary>
    /// <remarks>File should start with '{' and end with '}'</remarks>
    /// <param name="file">The path of the JSON file</param>
    /// <returns>The JSON data</returns>
    public static JObject ReadFromFile(string file)
    {
        StreamReader sr = new(file);
        JsonTextReader reader = new(sr);
        var ret = JObject.Load(reader);
        reader.Close();
        sr.Close();
        return ret;
    }

    /// <summary>
    /// The method saves the JObject to the file
    /// </summary>
    /// <param name="file">the exact path of the file</param>
    /// <param name="obj">the JObject JSON data</param>
    public static void SaveFile(string file, JObject obj)
    {
        StreamWriter sw = new(file);
        JsonTextWriter writer = new(sw);
        obj.WriteTo(writer);
        sw.Close();
        writer.Close();
    }

    /// <summary>
    /// The method merges the 2 JObjects.
    /// The result will be stored in target
    /// </summary>
    /// <param name="target">the target JSON data</param>
    /// <param name="needtomerge">the JSON data that needs to merge to the target</param>
    public static void MergeJObject(this JObject target, JObject needtomerge)
    {
        foreach (var item in needtomerge)
        {
            if (target[item.Key] == null)
            {
                target.Add(item.Key, item.Value);
                continue;
            }
            if (item.Value == null) continue;
            MergeEachJObject(target, item.Value, item.Key);
        }
    }

    private static void MergeEachJObject(this JObject target, JToken needtomerge, string key)
    {
        if (target == null) return;
        switch (target[key]?.Type)
        {
            case JTokenType.Object:
                MergeJObject(
                    target[key] is JObject jobject ? jobject : [], (JObject)needtomerge);
                break;
            case JTokenType.Array:
                MergeJArray(
                    target[key] is JArray array ? array : [], (JArray)needtomerge);
                break;
            default:
                target[key] = needtomerge;
                break;
        }
    }

    /// <summary>
    /// The method merges the 2 JArrays.
    /// It will append the needtomerge array at the end of target array
    /// </summary>
    /// <param name="target">The target JArray</param>
    /// <param name="needtomerge">The array that needs to append</param>
    public static void MergeJArray(this JArray target, JArray needtomerge)
    {
        foreach (var item in needtomerge)
            if (!target.Contains(item)) target.Add(item);
    }

    /// <summary>
    /// Tries to get a value from the JObject at the specified path and formats it.
    /// </summary>
    /// <typeparam name="T">The type of the value to be returned.</typeparam>
    /// <param name="obj">The JObject to search.</param>
    /// <param name="format">The function to format the JToken to the desired type.</param>
    /// <param name="path">The path to the value in the JObject.</param>
    /// <param name="output">The output value if found and formatted successfully.</param>
    /// <returns>True if the value was found and formatted successfully, otherwise false.</returns>
    public static bool TryGetValue<T>(this JObject obj, Func<JToken, T> format, string path, out T? output)
    {
        output = default;
        var tok = GetFromPath(obj, path);
        if (tok == null) return false;
        try { output = format(tok); } catch { return false; }
        return true;
    }

    /// <summary>
    /// Gets a value from the JObject at the specified path and formats it.
    /// </summary>
    /// <typeparam name="T">The type of the value to be returned.</typeparam>
    /// <param name="obj">The JObject to search.</param>
    /// <param name="format">The function to format the JToken to the desired type.</param>
    /// <param name="path">The path to the value in the JObject.</param>
    /// <returns>The formatted value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the path is not found in the JObject.</exception>
    public static T GetValue<T>(this JObject obj, Func<JToken, T> format, string path)
    {
        var tok = GetFromPathCheck(obj, path);
        return format(tok);
    }

    /// <summary>
    /// Gets a value from the JObject at the specified path and formats it, or returns a default value if the path is not found.
    /// </summary>
    /// <typeparam name="T">The type of the value to be returned.</typeparam>
    /// <param name="obj">The JObject to search.</param>
    /// <param name="format">The function to format the JToken to the desired type.</param>
    /// <param name="path">The path to the value in the JObject.</param>
    /// <param name="defaultVal">The default value to return if the path is not found.</param>
    /// <returns>The formatted value or the default value.</returns>
    public static T GetOrDefault<T>(this JObject obj, Func<JToken, T> format, string path, T defaultVal)
    {
        T ret;
        var tok = GetFromPath(obj, path);
        if (tok == null) return defaultVal;
        try { ret = format(tok); } catch { return defaultVal; }
        return ret;
    }

    /// <summary>
    /// Gets a JToken from the JObject at the specified path.
    /// </summary>
    /// <param name="obj">The JObject to search.</param>
    /// <param name="path">The path to the value in the JObject.</param>
    /// <returns>The JToken at the specified path, or null if not found.</returns>
    public static JToken? GetFromPath(this JObject obj, string path)
    {
        JToken? token = obj;
        var keys = path.Split('/');
        var ind = 0;
        while (ind < keys.Length)
        {
            var key = keys[ind++];
            if (key == "") continue;
            token = token.Type switch
            {
                JTokenType.Object => (token as JObject)?[key],
                JTokenType.Array => token[int.Parse(key)],
                _ => null,
            };
            if (token == null) return null;
        }
        return token;
    }

    /// <summary>
    /// Retrieves a <see cref="JToken"/> from the specified JSON path. 
    /// Throws an exception if the token does not exist.
    /// </summary>
    /// <param name="obj">The <see cref="JObject"/> to search within.</param>
    /// <param name="path">The JSON path to the desired token.</param>
    /// <returns>The retrieved <see cref="JToken"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token does not exist at the specified path.</exception>
    public static JToken GetFromPathCheck(this JObject obj, string path)
    {
        return obj.GetFromPath(path) ??
               throw new InvalidOperationException("Token does not exist");
    }

    /// <summary>
    /// Converts a <see cref="JToken"/> to the specified type. 
    /// Returns a default value if the token is null.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="token">The <see cref="JToken"/> to convert, which can be null.</param>
    /// <param name="format">A conversion function that transforms the <see cref="JToken"/> into the target type.</param>
    /// <param name="defaultVal">The default value to return if the token is null.</param>
    /// <returns>The converted value, or the default value if the token is null.</returns>
    public static T ConvertTo<T>(this JToken? token, Func<JToken, T> format, T defaultVal)
    {
        if (token == null) return defaultVal;
        return format(token);
    }

    /// <summary>
    /// Converts a <see cref="JArray"/> to a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the resulting list.</typeparam>
    /// <param name="array">The <see cref="JArray"/> to convert.</param>
    /// <param name="format">A function that converts each <see cref="JToken"/> to <typeparamref name="T"/>.</param>
    /// <returns>A <see cref="List{T}"/> containing the converted elements.</returns>
    public static List<T> ToList<T>(this JArray array, Func<JToken, T> format)
        => [.. array.Select(format)];

    /// <summary>
    /// Converts a path to a key by replacing slashes with colons.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string PathToKey(string path)
        => path.Replace("/", ":").Replace("\\", ":");
}

/// <summary>
/// This class provides some functions that converts JToken
/// to other classes
/// </summary>
public static class Format
{
    /// <summary>
    /// Converts the JToken to string
    /// </summary>
    /// <param name="tok">The token that needed to convert</param>
    /// <returns>the result of convert</returns>
    public static string ToString(JToken tok)
        => tok.ToString();

    /// <summary>
    /// Converts a <see cref="JToken"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="tok">The <see cref="JToken"/> to convert.</param>
    /// <returns>The integer value of the token.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not of type <see cref="JTokenType.Integer"/>.</exception>
    public static int ToInt32(JToken tok)
    {
        if (tok.Type == JTokenType.Integer) return (int)tok;
        throw new InvalidOperationException("Token is not an Integer");
    }

    /// <summary>
    /// Converts a <see cref="JToken"/> to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="tok">The <see cref="JToken"/> to convert.</param>
    /// <returns>The <see cref="DateTime"/> value of the token.</returns>
    /// <exception cref="FormatException">Thrown if the token cannot be parsed as a valid date-time string.</exception>
    public static DateTime ToDateTime(JToken tok)
        => DateTime.Parse(tok.ToString());

    /// <summary>
    /// Converts a <see cref="JToken"/> to a <see cref="bool"/>.
    /// </summary>
    /// <param name="tok">The <see cref="JToken"/> to convert.</param>
    /// <returns>The boolean value of the token.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not of type <see cref="JTokenType.Boolean"/>.</exception>
    public static bool ToBool(JToken tok)
    {
        if (tok.Type == JTokenType.Boolean) return (bool)tok;
        throw new InvalidOperationException("Token is not a bool");
    }

    /// <summary>
    /// Converts a <see cref="JToken"/> to a <see cref="double"/>.
    /// </summary>
    /// <param name="tok">The <see cref="JToken"/> to convert.</param>
    /// <returns>The double value of the token.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not of type <see cref="JTokenType.Float"/>.</exception>
    public static double ToDouble(JToken tok)
    {
        if (tok.Type == JTokenType.Float) return (double)tok;
        throw new InvalidOperationException("Token is not a double");
    }

    /// <summary>
    /// Converts a <see cref="JToken"/> to a <see cref="JObject"/>.
    /// </summary>
    /// <param name="tok">The <see cref="JToken"/> to convert.</param>
    /// <returns>The <see cref="JObject"/> representation of the token.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not of type <see cref="JTokenType.Object"/>.</exception>
    public static JObject ToJObject(JToken tok)
    {
        if (tok.Type == JTokenType.Object) return (JObject)tok;
        throw new InvalidOperationException("Token is not an object");
    }

    /// <summary>
    /// Converts a <see cref="JToken"/> to a <see cref="JArray"/>.
    /// </summary>
    /// <param name="tok">The <see cref="JToken"/> to convert.</param>
    /// <returns>The <see cref="JArray"/> representation of the token.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not of type <see cref="JTokenType.Array"/>.</exception>
    public static JArray ToJArray(JToken tok)
    {
        if (tok.Type == JTokenType.Array) return (JArray)tok;
        throw new InvalidOperationException("Token is not an array");
    }
}