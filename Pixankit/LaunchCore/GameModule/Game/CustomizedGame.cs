using PixanKit.LaunchCore.GameModule.Folders;
namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
/// Modified Game. Game With ModLoader/ Optifine
/// </summary>
public class CustomizedGame : GameBase
{
    /// <summary>
    /// 
    /// </summary>
    public string InheritsFrom { get; protected set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool UseBaseGeneration { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="folder"></param>
    /// <param name="version"></param>
    /// <param name="inheritsFrom"></param>
    /// <param name="useBaseGeneration"></param>
    public CustomizedGame(string name, Folder folder,
        string version, string inheritsFrom, bool useBaseGeneration = true) :
        base(name, folder, version)
    {
        GameType = GameType.Customized;
        InheritsFrom = inheritsFrom;
        UseBaseGeneration = useBaseGeneration;
    }
}