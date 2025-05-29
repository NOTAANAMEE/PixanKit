using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core.Managers;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
///
/// </summary>
public struct GameSettings()
{
    /// <summary></summary>
    public SettingValue JavaSetting;
    /// <summary></summary>
    public SettingValue RunningFolderSetting;
    /// <summary></summary>
    public SettingValue JVMArgSetting;
    /// <summary></summary>
    public SettingValue PreArgSetting;
    /// <summary></summary>
    public SettingValue PostArgSetting;
    /// <summary></summary>
    public SettingValue EnvVariableSetting;

    /// <summary></summary>
    public string CustomJavaSetting = "";
    /// <summary></summary>
    public string CustomRunningFolderSetting = "";
    /// <summary></summary>
    public string Description = "";
    /// <summary></summary>
    public string CustomJVMArgSetting = "";
    /// <summary></summary>
    public string CustomPreArgSetting = "";
    /// <summary></summary>
    public string CustomPostArgSetting = "";
    /// <summary>Launching variables</summary>
    public List<KeyValuePair<string, string>> Variables = [];

    /// <summary>
    /// Initialize from Json Data
    /// </summary>
    /// <param name="obj">The Json Data</param>
    public GameSettings(JObject obj) : this()
    {
        JavaSetting = obj.GetOrDefault(ToSettingValue, 
            "java", SettingValue.Overall);
        RunningFolderSetting = obj.GetOrDefault(ToSettingValue, 
            "running_folder", SettingValue.Overall);
        JVMArgSetting = obj.GetOrDefault(ToSettingValue, 
            "jvm_argument", SettingValue.Overall);
        PreArgSetting = obj.GetOrDefault(ToSettingValue, 
            "pre_argument", SettingValue.Overall);
        PostArgSetting = obj.GetOrDefault(ToSettingValue, 
            "post_argument", SettingValue.Overall);
        EnvVariableSetting = obj.GetOrDefault(ToSettingValue,
            "env_variables", SettingValue.Overall);
        
        CustomJavaSetting = obj.GetOrDefault(
            Format.ToString, "custom_java", "");
        CustomRunningFolderSetting = obj.GetOrDefault(
            Format.ToString, "custom_running_folder", "");
        Description = obj.GetOrDefault(
            Format.ToString, "description", "A Minecraft Game");
        CustomJVMArgSetting = obj.GetOrDefault(
            Format.ToString, "custom_jvm_argument", "");
        CustomPreArgSetting = obj.GetOrDefault(
            Format.ToString, "custom_pre_argument", "");
        CustomPostArgSetting = obj.GetOrDefault(
            Format.ToString, "custom_post_argument", "");
        Description = obj.GetOrDefault(
            Format.ToString, "description", "A Minecraft Game");
        Variables = obj.GetOrDefault(
            ToVar, "custom_env_variables", []);
    }
    
    private SettingValue ToSettingValue(JToken token)
    {
        return (SettingValue)(int)token;
    }

    private List<KeyValuePair<string, string>> ToVar(JToken token)
    {
        if (token is not JObject obj) throw new Exception();
        return obj.Properties()
            .Select(a => 
                new KeyValuePair<string, string>(a.Name, a.Value.ToString()))
            .ToList();
    }

    private JObject VarToJObject()
    {
        JObject obj = new();
        foreach (var pair in Variables) obj.Add(pair.Key, pair.Value);
        return obj;
    }

    /// <summary>
    /// To JObject
    /// </summary>
    /// <returns>To JObject</returns>
    public JObject ToJObject()
    {
        return new()
        {
            { "java", (int)JavaSetting },
            { "running_folder", (int)RunningFolderSetting },
            { "jvm_argument", (int)JVMArgSetting },
            { "pre_argument", (int)PreArgSetting },
            { "post_argument", (int)PostArgSetting },
            { "env_variables", (int)EnvVariableSetting },
            { "custom_java", CustomJavaSetting },
            { "custom_running_folder", CustomRunningFolderSetting },
            { "description", Description },
            { "custom_jvm_argument", CustomJVMArgSetting },
            { "custom_pre_argument", CustomPreArgSetting },
            { "custom_post_argument", CustomPostArgSetting },
            { "custom_env_variables", VarToJObject() },
        };
    }
}

/// <summary>
/// The set of all possible setting values
/// </summary>
public enum SettingValue
{
    /// <summary>the same as the Launcher's setting</summary>
    Overall,
    /// <summary>use the game folder as the savings folder</summary>
    Self,
    /// <summary>use the .minecraft folder as the savings folder</summary>
    Folder,
    /// <summary>use the latest version of Java</summary>
    Latest,
    /// <summary>the closest version of Java</summary>
    Closest,
    /// <summary>use the specified version of Java</summary>
    Specified,
    /// <summary>Append custom params to the overall params</summary>
    Append,
    /// <summary>user customs the setting</summary>
    Custom,
}
