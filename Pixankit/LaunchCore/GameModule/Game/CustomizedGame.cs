using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.SystemInf;

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
    protected GameParameter ThisParameter;
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="folder"></param>
    /// <param name="param"></param>
    /// <param name="libs"></param>
    public CustomizedGame(string name, Folder folder, 
        GameParameter param, LibrariesRef libs) : 
        base(name, folder, param, libs)
    {
        ThisParameter = param;
        GameType = GameType.Customized;
        _useBaseGeneration = param.ReliedArgs;
    }

    private LibrariesRef? _libs;
    private GameParameter? _gameArgs;

    private GameParameter GetGameArgs()
    {
        if (_gameArgs != null) return _gameArgs;

        if (!Launcher.Instance.GameManager.TryGetParam(Version,
                out var param, out var libs))
            throw new Exception();
        _libs = libs;
        _gameArgs = param;
        return _gameArgs;
    }
        
    private LibrariesRef GetLibrariesRef()
    {
        if (_libs != null) return _libs;

        if(!Launcher.Instance.GameManager.TryGetParam(Version, 
               out var param, out var libs)) 
            throw new Exception();
        _libs = libs;
        _gameArgs = param;
        return _libs;
    }

    /// <inheritdoc/>
    protected override string GetAssetsId()
    {
        return _useBaseGeneration ? GetGameArgs().AssetsId : Params.AssetsId;
    }

    /// <inheritdoc/>
    protected override string GetArgument()
    {
        return _useBaseGeneration?
            GetGameArgs().GameArgs + base.GetArgument() + GetGameArgs().GameArgs : 
            base.GetArgument();
    }

    /// <inheritdoc/>
    protected override string GetCpArgs()
    {
        return _useBaseGeneration ? 
            base.GetCpArgs() + Localize.LocalParser + GetCpArgs(GetLibrariesRef().Libraries): 
            base.GetCpArgs();
    }
}