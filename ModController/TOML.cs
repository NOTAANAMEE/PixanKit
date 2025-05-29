using Tomlyn.Model;

namespace PixanKit.ModController;

internal static class Toml
{
    public static object? GetPath(this TomlTable table, string path)
    {
        var keys = path.Split('/');
        object value = table;
        foreach (var key in keys)
        {
            if (key == "") continue;
            switch (value)
            {
                case TomlTable tomlTable:
                    if (!tomlTable.TryGetValue(key, out value)) return null;
                    break;
                case TomlTableArray tomlArray:
                    var success = int.TryParse(key, out var index);
                    if (!success) return null;
                    if (tomlArray.Count <= index) return null;
                    value = tomlArray[index];
                    break;
                default:
                    return null;
            }
        }
        return value;
    }

    public static T GetValue<T>(this TomlTable table, string path)
    {
        var val = table.GetPath(path);
        if (val == null || val.GetType() != typeof(T))
            throw new InvalidOperationException("Not valid");
        return (T)val;
    }

    public static T GetOrDefault<T>(this TomlTable table, string path, T defaultVal)
    {
        var val = table.GetPath(path);
        if (val == null || val.GetType() != typeof(T))
            return defaultVal;
        return (T)val;
    }

    public static bool TryGet<T>(this TomlTable table, string path, out T? output)
    {
        output = default;
        var val = table.GetPath(path);
        if (val == null || val.GetType() != typeof(T))
            return false;
        output = (T)val;
        return true;
    }
}