using PixanKit.LaunchCore.GameModule.Library;
using PixanKit.LaunchCore.GameModule.Folders;

namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
/// Represents the Vanilla Minecraft game without Optifine or mod loaders.
/// </summary>
public class VanillaGame : GameBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VanillaGame"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="folder"></param>
    /// <param name="version"></param>
    public VanillaGame(string name, Folder folder,
        string version) :
        base(name, folder, version)
    { }
}
