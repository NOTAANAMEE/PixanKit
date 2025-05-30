﻿using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.LibraryData;
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
    public string ModLoader = "quilt";

    /// <inheritdoc/>
    public ModdedGame(string name, Folder folder,
        GameParameter param, LibrariesRef libs):
        base(name, folder, param, libs)
    {
        GameType = GameType.Modded;
        SetModLoader(param);
    }

    private void SetModLoader(GameParameter param)
    {
        if (param.JavaArgs.Contains("fabric")) 
            ModLoader = "fabric";
        else if (param.GameArgs.Contains("neoForge")) 
            ModLoader = "neoforge";
        else if (param.GameArgs.Contains("forge")) 
            ModLoader = "forge";
    }
}