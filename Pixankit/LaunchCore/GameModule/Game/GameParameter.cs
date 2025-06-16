using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Library;
using PixanKit.LaunchCore.Json;
using System.Text;

namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
/// 
/// </summary>
public partial class GameParameter
{

    /// <summary>
    /// Gets the game version.
    /// </summary>
    public string Version { get; }

    public partial bool IsModified => _type != 0;

    public partial bool ReliedArgs => _type == 1;

    public partial string JavaArgs => _javaArgs ?? "";
    
    public partial string GameArgs => _gameArgs ?? "";
    
    public partial string MainClass => _mainClass;
    
    public partial string AssetsId => _assetsId ?? "";
    
    public partial short JvmVersion { get => _jvmVersion; private set => _jvmVersion = value; }

    private string? _javaArgs;
        
    private string? _gameArgs;

    private string? _assetsId;
        
    private int _type;
    
    private short _jvmVersion;

    private string _mainClass;

    private GameParameter(string version, JObject jData)
    {
        Version = version;
        _mainClass = 
            (jData["mainClass"] ?? "net.minecraft.client.main.Main.")
            .ToString();
        SetAssetsId(jData);
        SetGameArgs(jData);
        SetJavaArgs(jData);
        SetJvmVersion(jData);
    }

    public static partial GameParameter CreateInstance(JObject jData)
    {
        var version = GetVersion(jData, out var modified);
        return new(version, jData) {_type = modified};
    }
        
    private void SetGameArgs(JObject obj)
    {
        if (obj.TryGetValue(Format.ToString, "minecraftArguments", out var gargs))
        {
            _gameArgs = "" + gargs;
            return;
        }
        var array = obj.GetOrDefault(Format.ToJArray,
            "arguments/game", []);
        StringBuilder builder = new();
        foreach (var token in array)
        {
            if (token.Type == JTokenType.String) 
            {
                builder.Append(token + " ");
            }
        }
        _gameArgs = builder.ToString();
    }

    private void SetJavaArgs(JObject jData)
    {
        StringBuilder builder = new();
        foreach (var token in GetArgArray(jData))
        {
            var arg = ParseArg(token);
            if (arg.Length != 0)
                builder.Append($"{arg} ");
        }
        _javaArgs = builder.ToString();
    }

    private void SetAssetsId(JObject jData)
    {
        _assetsId = jData.GetOrDefault(Format.ToString,
            "assetIndex/id", "");
    }

    private void SetJvmVersion(JObject jData)
    {
        JvmVersion = (short)jData.GetOrDefault(Format.ToInt32,
            "javaVersion/majorVersion", 0);
    }
        
    private static JArray GetArgArray(JObject jData)
    {
        const string defaultJson = "[{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"osx\"}}],\"value\": [\"-XstartOnFirstThread\"]},{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"windows\"}}],\"value\": \"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump\"},{\"rules\": [{\"action\": \"allow\",\"os\": {\"arch\": \"x86\"}}],\"value\": \"-Xss1M\"},\"-Djava.library.path=${natives_directory}\"" +
                                   ",\"-Dminecraft.launcher.brand=${launcher_name}\",\"-Dminecraft.launcher.version=${launcher_version}\",\"-cp\",\"${classpath}\"]";
        if (jData.TryGetValue(Format.ToJArray, "arguments/jvm", out var array))
            return array ?? [];
        return JArray.Parse(defaultJson);
    }
        
    private static string ParseArg(JToken token)
    {
        if (token.Type == JTokenType.String)
        {
            var arg = token.ToString();
            return arg.Contains(' ') ? $"\"{arg}\"" : arg;
        }

        if (token.Type != JTokenType.Object ||
            !LibraryHelper.SystemSupport((JObject)token)) return "";
   
        var obj = (JObject)token;
        var value = obj.GetFromPathCheck("value");

        return value.Type switch
        {
            JTokenType.String => value.ToString(),
            JTokenType.Array => string.Join(" ", value.Select(v => v.ToString())),
            _ => ""
        };
    }
    
    public static partial string GetVersion(JObject jData, out int isModified)
    {
        isModified = 0;
        if (jData.TryGetValue("inheritsFrom", out var inheritsFrom))
        {
            isModified = 1;
            return inheritsFrom.ToString();
        }
        if (!jData.TryGetValue("clientVersion", out var clientVersion))
            return (jData["id"] ?? "").ToString();
            
        isModified = 2;
        return clientVersion.ToString();
    }
}