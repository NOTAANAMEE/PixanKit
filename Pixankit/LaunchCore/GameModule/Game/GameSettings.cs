using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.Game;

public struct GameSettings()
{
    public readonly bool IsOverall = false;
    
    public SettingValue JavaSetting;
    public SettingValue RunningFolderSetting;
    public SettingValue JVMArgSetting;
    public SettingValue PreArgSetting;
    public SettingValue PostArgSetting;
    public SettingValue EnvVariableSetting;

    public string CustomJavaSetting = "";    
    public string CustomRunningFolderSetting = "";
    public string Description = "";
    public string CustomJVMArgSetting = "";
    public string CustomPreArgSetting = "";
    public string CustomPostArgSetting = "";
    
    public List<KeyValuePair<string, string>> Variables = [];

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

public enum SettingValue
{
    Overall,
    Custom,
    Self,
    Latest,
    Closest,
    Specified,
    Append
}
