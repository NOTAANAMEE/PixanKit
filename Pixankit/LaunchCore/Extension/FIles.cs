using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace PixanKit.LaunchCore.Extension;

/// <summary>
/// Some Settings About Files
/// </summary>
public static class Files
{
    static Files()
    {
        ConfigDir = "./Launcher/Config";
        CacheDir = "./Launcher/Cache";
        SettingsPath = "/Launcher/settings.json";
        ManifestDir = "${CacheDir}/manifest.json";
        SkinCacheDir = "${CacheDir}/Skin";
    }

    /// <summary>
    /// Folder JSON Data  
    /// </summary>
    public static JObject FolderJData { get; set; } = DefaultJson.JData;

    /// <summary>
    /// Runtime JSON Data
    /// </summary>
    public static JObject RuntimeJData { get; set; } = DefaultJson.JData;

    /// <summary>
    /// Player JSON Data
    /// </summary>
    public static JObject PlayerJData { get; set; } = DefaultJson.JData;

    /// <summary>
    /// Setting JSON Data
    /// </summary>
    public static JObject? SettingsJData { get; set; } = DefaultJson.JData;

    /// <summary>
    /// Directory For Launcher Configuration Files.
    /// </summary>
    public static string ConfigDir
    { get => Paths.Get("ConfigDir"); set => Paths.TrySet("ConfigDir", value); }

    /// <summary>
    /// Dir For Cache
    /// </summary>
    public static string CacheDir
    { get => Paths.Get("CacheDir"); set => Paths.TrySet("CacheDir", value); }

    /// <summary>
    /// The native setting for every game. 
    /// For example:C:\Users\admin\AppData\Roaming\.minecraft\versions\1.20.4\settings.json
    /// </summary>
    public static string SettingsPath
    { get => Paths.Get("SettingsPath"); set => Paths.TrySet("SettingsPath", value); }

    /// <summary>
    /// Dir For Minecraft Version Manifest
    /// </summary>
    public static string ManifestDir
    { get => Paths.Get("ManifestDir"); set => Paths.TrySet("ManifestDir", value); }

    /// <summary>
    /// Dir For Skin Cache
    /// </summary>
    public static string SkinCacheDir
    { get => Paths.Get("SkinCacheDir"); set => Paths.TrySet("SkinCacheDir", value); }

    /// <summary>
    /// Get SHA1 From File
    /// </summary>
    /// <param name="path">File Path</param>
    /// <returns>SHA1 String</returns>
    public static string GetSha1(string path)
    {
        FileStream fs = new(path, FileMode.Open);
        var sha1 = SHA1.Create();
        var ret = sha1.ComputeHash(fs);
        fs.Close();
        return BitConverter.ToString(ret).Replace("-", "");
    }

    /// <summary>
    /// Generate Default JSON Data
    /// </summary>
    public static void Generate()
    {
        FolderJData = (JObject)DefaultJson.JData.DeepClone();
        PlayerJData = (JObject)FolderJData.DeepClone();
        RuntimeJData = (JObject)FolderJData.DeepClone();
        RuntimeJData.Remove("target");
        SettingsJData = (JObject)DefaultJson.SettingJData.DeepClone();
    }

    /// <summary>
    /// Save JSON to default path.<br/>
    /// This method is intended for simple save operations only 
    /// and is not suitable for encrypted storage or other complex operations.
    /// It is recommended to use encrypted storage when saving Player data.
    /// </summary>
    public static void Save()
    {
        FileStream folderFs = new($"{ConfigDir}/Folders.json", FileMode.Create),
            playerFs = new($"{ConfigDir}/Players.json", FileMode.Create),
            runtimeFs = new($"{ConfigDir}/JavaRuntime.json", FileMode.Create),
            settingsFs = new($"{ConfigDir}/Settings.json", FileMode.Create);
        StreamWriter foldersw = new(folderFs),
            playersw = new(playerFs),
            runtimesw = new(runtimeFs),
            settingsw = new(settingsFs);
        foldersw.Write(FolderJData.ToString());
        playersw.Write(PlayerJData.ToString());
        runtimesw.Write(RuntimeJData.ToString());
        settingsw.Write(SettingsJData?.ToString() ?? "");
        foldersw.Close();
        playersw.Close();
        runtimesw.Close();
        settingsw.Close();
        folderFs.Close();
        playerFs.Close();
        runtimeFs.Close();
        settingsFs.Close();
    }

    /// <summary>
    /// Load Data From Default Path
    /// </summary>
    public static void Load()
    {
        FileStream folderFs = new($"{ConfigDir}/Folders.json", FileMode.Open),
            playerFs = new($"{ConfigDir}/Players.json", FileMode.Open),
            runtimeFs = new($"{ConfigDir}/JavaRuntime.json", FileMode.Open),
            settingsFs = new($"{ConfigDir}/Settings.json", FileMode.Open);
        StreamReader foldersr = new(folderFs),
            playersr = new(playerFs),
            runtimesr = new(runtimeFs),
            settingsr = new(settingsFs);
        Task<string> t1 = foldersr.ReadToEndAsync(),
            t2 = playersr.ReadToEndAsync(),
            t3 = runtimesr.ReadToEndAsync(),
            t4 = settingsr.ReadToEndAsync();
        Task.WaitAll(t1, t2, t3, t4);
        FolderJData = JObject.Parse(t1.Result);
        PlayerJData = JObject.Parse(t2.Result);
        RuntimeJData = JObject.Parse(t3.Result);
        SettingsJData = JObject.Parse(t4.Result);
        folderFs.Close();
        playerFs.Close();
        runtimeFs.Close();
        settingsFs.Close();
    }
}

internal static class DefaultJson
{
    public static JObject JData = new()
    {
        { "children", new JArray() },
        { "target", "" }
    };

    public static JObject SettingJData = new()
    {
        { "java", "closest" },
        { "arguments", "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Dlog4j2.formatMsgNoLookups=true" },
        { "runningfolder", "self" }
    };
}