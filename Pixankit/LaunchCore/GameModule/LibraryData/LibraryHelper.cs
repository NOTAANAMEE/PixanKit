using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.GameModule.LibraryData;

public static class LibraryHelper
{
    /// <summary>
    /// Checks whether the system suits for the argument or library
    /// </summary>
    /// <param name="libraryToken"></param>
    /// <returns></returns>
    public static bool SystemSupport(JObject libraryToken)
        => GetAllowedSystem(libraryToken).Contains(SysInfo.OsName);

    /// <summary>
    /// This is for judging which system is suitable for this library
    /// </summary>
    /// <param name="jData">The Library Json Data. Like <br/><c>
    /// {
    ///"downloads": {
    /// "artifact": {
    ///  "path": "org/slf4j/slf4j-api/2.0.9/slf4j-api-2.0.9.jar",
    ///  "sha1": "7cf2726fdcfbc8610f9a71fb3ed639871f315340",
    ///  "size": 64579,
    ///  "url": "https://libraries.minecraft.net/org/slf4j/slf4j-api/2.0.9/slf4j-api-2.0.9.jar"
    /// }
    /// },
    ///"name":"org:slf4j:slf4j-api:2.0.9"
    ///},</c></param>
    /// <returns></returns>
    public static HashSet<string> GetAllowedSystem(JObject jData)
    {
        if (jData["rules"] == null) return ["osx", "linux", "windows"];

        HashSet<string> osSet = [];

        foreach (var ruleData in jData.GetOrDefault(Format.ToJArray, "rules", []))
        {
            var jObj = ruleData.ConvertTo(Format.ToJObject, []);

            var action = jObj.GetOrDefault(Format.ToString, "action", "");
            var osData = jObj.GetOrDefault(Format.ToJObject, "os", null);
            var osName = jObj.GetOrDefault(Format.ToString, "os/name", null);
            var osArch = jObj.GetOrDefault(Format.ToString, "os/arch", null);

            switch (action)
            {
                case "allow":
                    if (osData == null) osSet = ["osx", "linux", "windows"];
                    else if (osName != null) osSet.Add(osName);
                    if (osArch != null && osArch != SysInfo.CpuArch) return [];
                    break;
                case "disallow":
                    if (osName != null) osSet.Remove(osName);
                    break;
            }
        }
        return [.. osSet];
    }

    /// <summary>
    /// This is for judging which library type the library is
    /// </summary>
    /// <param name="jData"></param>
    /// <returns></returns>
    public static LibraryType GetLibraryType(JToken jData)
    {
        if (jData["natives"] != null) return LibraryData.LibraryType.Native;
        if (jData["downloads"] != null) return LibraryData.LibraryType.Default;
        return LibraryData.LibraryType.Mod;
    }

    /// <summary>
    /// Get The Path Of The Library
    /// </summary>
    /// <param name="name">Name Like <c>"com.mojang:logging:1.4.9"</c></param>
    /// <returns>Path Of The Library. Like 
    /// <c>"/com/mojang/logging/1.4.9/logging-1.4.9.jar"</c></returns>
    public static string GetPath(string name)
    {
        if (name.Contains('/')) return name;
        var pathInf = name;
        var strings = pathInf.Split(":");
        strings[^1] = strings[^1].Replace(".jar", "");
        return strings.Length switch
        {
            3 => $"{strings[0].Replace('.', '/')}/{strings[1]}/{strings[2]}/{strings[1]}-{strings[2]}.jar",
            4 => $"{strings[0].Replace('.', '/')}/{strings[1]}/{strings[2]}/{strings[1]}-{strings[2]}-{strings[3]}.jar",
            _ => name
        };
    }

    private static string GetName(JObject jData)
        => (jData["name"] ?? "").ToString();
    
    /// <summary>
    /// Parses the library according to the JSON data
    /// </summary>
    /// <param name="jData">the JSON data of the object</param>
    /// <param name="gamelibraries">The list of the library</param>
    public static void AddLibrary(JObject jData, List<LibraryBase> gamelibraries)
    {
        if (!LibraryHelper.SystemSupport(jData)) return;
        var type = LibraryHelper.GetLibraryType(jData);
        LibraryBase? library;
        if (type == LibraryType.Native)
        {
            DefaultLibrary.CreateInstance(jData, out library);
            gamelibraries.Add(library);
        }
            
        switch (type)
        {
            case LibraryData.LibraryType.Default:
                DefaultLibrary.CreateInstance(jData, out library);
                break;
            case LibraryData.LibraryType.Native:
                if (!NativeLibrary.CreateInstance(jData, out library)) return;
                break;
            case LibraryData.LibraryType.Mod:
                LoaderLibrary.CreateInstance(jData, out library);
                break;
            default:
                return;
        }
        gamelibraries.Add(library);
        
    }
}
