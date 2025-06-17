using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.Folders;

namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
/// Minecraft Game With Mod Loader
/// </summary>
public class ModdedGame : CustomizedGame
{
    /// <summary>
    /// The Mod path. For instance: C:\Users\Admin\AppData\.minecraft\versions\1.12.2-Forge\mods
    /// </summary>
    public string ModDir => 
        $"{Launcher.Instance.GetGameRunningFolder(this)}mods/";

    /// <summary>
    /// Stores the name of the mod loader
    /// </summary>
    public string ModLoader;

    /// <inheritdoc/>
    public ModdedGame(string name, Folder folder, 
        string version, string inheritsFrom, string modLoader, bool modified):
        base(name, folder, version, inheritsFrom, modified)
    {
        GameType = GameType.Modded;
        ModLoader = modLoader;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public static string SetModLoader(GameParameter param)
    {
        if (param.JavaArgs.Contains("fabric")) 
            return "fabric";
        if (param.GameArgs.Contains("neoForge")) 
            return "neoforge";
        if (param.GameArgs.Contains("forge")) 
            return "forge";
        return "quilt";
    }
}