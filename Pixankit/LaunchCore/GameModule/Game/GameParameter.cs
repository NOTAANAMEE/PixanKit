using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Json;
using System.Text;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// 
    /// </summary>
    public class GameParameter
    {

        /// <summary>
        /// Gets the game version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the game type.
        /// </summary>
        public bool IsModified => _type != 0;

        /// <summary>
        /// Decide whether the arguments should be based on Vanilla game arguments
        /// </summary>
        public bool ReliedArgs => _type == 1;

        /// <summary>
        /// Gets the Java arguments.
        /// </summary>
        public string JavaArgs => _javaArgs ?? "";

        /// <summary>
        /// Gets the game arguments.
        /// </summary>
        public string GameArgs => _gameArgs ?? "";

        /// <summary>
        /// Gets the entry class
        /// </summary>
        public string? MainClass { get; }
        
        /// <summary>
        /// Gets the Assets ID of a game
        /// </summary>
        public string AssetsId => _assetsId ?? "";

        /// <summary>
        /// JVM Version
        /// </summary>
        public short JvmVersion { get; private set; }

        private string? _javaArgs;
        
        private string? _gameArgs;

        private string? _assetsId;
        
        private int _type;

        private GameParameter(string version, JObject jData)
        {
            Version = version;
            MainClass = 
                (jData["mainClass"] ?? "net.minecraft.client.main.Main.")
                .ToString();
            SetAssetsId(jData);
            SetGameArgs(jData);
            SetJavaArgs(jData);
            SetJvmVersion(jData);
        }

        /// <summary>
        /// Create an instance of the GameParameter
        /// </summary>
        /// <param name="jData"></param>
        /// <returns></returns>
        public static GameParameter CreateInstance(JObject jData)
        {
            var version = GetVersion(jData, out var modified);
            return new(version, jData) {_type = modified};
        }
        
        private void SetGameArgs(JObject obj)
        {
            if (obj.TryGetValue(Format.ToString, "minecraftArguments", out string? gargs))
            {
                _gameArgs = "" + gargs;
                return;
            }
            var array = obj.GetOrDefault(Format.ToJArray,
                "arguments/game", []);
            StringBuilder builder = new();
            foreach (JToken token in array)
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
            foreach (JToken token in GetArgArray(jData))
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
            if (jData.TryGetValue(Format.ToJArray, "arguments/jvm", out JArray? array))
                return array ?? [];
            return JArray.Parse(defaultJson);
        }
        
        private static string ParseArg(JToken token)
        {
            if (token.Type == JTokenType.String)
            {
                string arg = token.ToString();
                return arg.Contains(' ') ? $"\"{arg}\"" : arg;
            }

            if (token.Type != JTokenType.Object ||
                !LibraryHelper.SystemSupport((JObject)token)) return "";
   
            JObject obj = (JObject)token;
            JToken value = obj.GetFromPathCheck("value");

            return value.Type switch
            {
                JTokenType.String => value.ToString(),
                JTokenType.Array => string.Join(" ", value.Select(v => v.ToString())),
                _ => ""
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jData"></param>
        /// <param name="isModified">
        /// Explanation:
        /// 0: It is the original game arguments, not modified.
        /// 1: It is modified by mod loaders and the args are incomplete
        /// 2: It is modified by mod loaders but the args are complete
        /// </param>
        /// <returns></returns>
        public static string GetVersion(JObject jData, out int isModified)
        {
            isModified = 0;
            if (jData.TryGetValue("inheritsFrom", out JToken? inheritsFrom))
            {
                isModified = 1;
                return inheritsFrom.ToString();
            }
            if (!jData.TryGetValue("clientVersion", out JToken? clientVersion))
                return (jData["id"] ?? "").ToString();
            
            isModified = 2;
            return clientVersion.ToString();
        }
    }
}