using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.GameModule.Library;

namespace PixanKit.LaunchCore.GameModule.Game;

/// <summary>
/// Modified Game. Game With ModLoader/ Optifine
/// </summary>
public class CustomizedGame : GameBase
{
    /// <summary>
    /// Whether It Is useBaseGeneration Created
    /// </summary>
    private readonly bool _useBaseGeneration;
        
    /// <summary>
    /// ThisParameter is the parameter from the local json.
    /// </summary>
    protected GameParameter BaseParameter;
    
    /// <summary>
    /// 
    /// </summary>
    public bool UseBaseGeneration => _useBaseGeneration;
    
    /// <summary>
    /// 
    /// </summary>
    public string InheritsFrom => "";
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="folder"></param>
    /// <param name="baseParam"></param>
    /// <param name="param"></param>
    /// <param name="baseLibrary"></param>
    /// <param name="libraries"></param>
    public CustomizedGame(string name, Folder folder,
        GameParameter baseParam,
        GameParameter param, 
        LibraryCollection baseLibrary,
        LibraryCollection libraries) :
        base(name, folder, param, libraries)
    {
        BaseParameter = baseParam;
        GameType = GameType.Customized;
        this.baseLibrary = baseLibrary;
        _useBaseGeneration = param.ReliedArgs;
    }

    private LibraryCollection? baseLibrary;

    /// <inheritdoc/>
    protected override string GetArgument()
    {
        return _useBaseGeneration?
            Params.JavaArgs + BaseParameter.JavaArgs + 
            $" {Params.MainClass} " + BaseParameter.GameArgs + Params.GameArgs:
            base.GetArgument();
    }

    /// <inheritdoc/>
    protected override string GetCpArgs()
    {
        if (!_useBaseGeneration) return base.GetCpArgs();
        var classpath = 
            string.Join("${classpath_separator}", 
                GetLibraries().Select(a => a.LibraryPath)) + "${classpath_separator}";
        if (baseLibrary is not null)
            classpath += string.Join("${classpath_separator}",
                baseLibrary.Libraries.Select(a => a.LibraryPath)) + "${classpath_separator}";
        classpath += GameJarFilePath;
        return classpath;
    }
}