using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.GameModule.Library;

/// <summary>
/// Static class for library helper methods
/// </summary>
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
}
