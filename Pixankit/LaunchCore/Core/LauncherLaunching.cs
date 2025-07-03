using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.GameModule.Param;
using PixanKit.LaunchCore.PlayerModule.Player;

namespace PixanKit.LaunchCore.Core;

public partial class Launcher
{
    /// <summary>
    /// Launch the game
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public LaunchSession.LaunchSession Launch(GameBase game)
    {
        return Launch(game, PlayerManager.TargetPlayer ??
                            throw new NullReferenceException("Target player not found"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public LaunchSession.LaunchSession Launch(GameBase game, PlayerBase player)
    {
        var cmd = InlineCommand(game, player);
        Logger.Logger.Info("Game Arg Generated Successfully.");
        var gameParam = ParameterManager.Instance.GetParameter(game);
        var java = JavaManager.ChooseRuntime(game, gameParam) ?? throw new NullReferenceException();

        //game.Decompress().Wait();

        return new LaunchSession.LaunchSession(game, java,
            GetPreArguments(game) ,cmd, 
            GetPostArguments(game), GetVariables(game));
    }

    /// <summary>
    /// Launch the default game
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public LaunchSession.LaunchSession Launch()
    {
        if (GameManager.TargetGame is null) 
            throw new NullReferenceException();
        return Launch(GameManager.TargetGame);
    }

    private string InlineCommand(GameBase game, PlayerBase player)
    {
        var cmd = ParameterManager.Instance.GenerateGameArg(game);

        cmd = PlayerInLine(cmd, player);
        cmd = $"-Xmx{Initers.GetMemory()}m " + cmd;
        cmd = cmd.Replace("${launcher_name}", LauncherName);
        cmd = cmd.Replace("${launcher_version}", VersionName);
        cmd = cmd.Replace("${game_directory}", GetGameRunningFolder(game));
        cmd = cmd.Replace("${jvm_argument}", GetJvmArguments(game));
        return cmd;
    }

    #region PlayerInline
    /// <summary>
    /// Inlines player information into a command string.
    /// </summary>
    /// <param name="arg">The base command string.</param>
    /// <param name="player">The player whose information is to be inlined.</param>
    /// <returns>The command string with the player's information inlined.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided player is null.</exception>
    public string PlayerInLine(string arg, PlayerBase player)
        => player.InlinePlayer(arg);

    /// <summary>
    /// Inlines the default player's information into a command string.
    /// </summary>
    /// <param name="arg">The base command string.</param>
    /// <returns>The command string with the default player's information inlined.</returns>
    public string PlayerInLine(string arg)
        => PlayerInLine(arg, PlayerManager.TargetPlayer ??
                             throw new NullReferenceException("Target player not found"));
    #endregion

    #region Getter
    private List<KeyValuePair<string, string>> GetVariables(GameBase game)
    {
        var tmp = game.Settings.EnvVariableSetting;
        var env = tmp switch
        {
            SettingValue.Custom => game.Settings.Variables,
            SettingValue.Append =>
                _settings.Variables.
                    UnionBy(game.Settings.Variables, pair => pair.Key)
                    .ToList(),
            _ => _settings.Variables,
        };
        return env;
    }
        
    private string GetPreArguments(GameBase game)
    {
        var postArg = game.Settings.PreArgSetting;
        return postArg switch
        {
            SettingValue.Custom => game.Settings.CustomPreArgSetting,
            SettingValue.Append => 
                _settings.PreArgSetting +
                game.Settings.CustomPreArgSetting,
            _ => _settings.CustomPreArgSetting,
        };
    }

    private string GetPostArguments(GameBase game)
    {
        var postArg = game.Settings.PostArgSetting;
        return postArg switch
        {
            SettingValue.Custom => game.Settings.CustomPostArgSetting,
            SettingValue.Append => 
                _settings.PostArgSetting +
                game.Settings.CustomPostArgSetting,
            _ => _settings.CustomPostArgSetting,
        };
    }

    private string GetJvmArguments(GameBase game)
    {
        var jvmArg = game.Settings.JVMArgSetting;
        return jvmArg switch
        {
            SettingValue.Custom => game.Settings.CustomJVMArgSetting,
            SettingValue.Append => 
                _settings.JVMArgSetting +
                game.Settings.CustomJVMArgSetting,
            _ => _settings.CustomJVMArgSetting,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    public string GetGameRunningFolder(GameBase game)
    {
        var folder = game.Settings.RunningFolderSetting;
        if (folder == SettingValue.Overall)
            folder = _settings.RunningFolderSetting;
        return folder switch
        {
            SettingValue.Custom => game.Settings.CustomRunningFolderSetting,
            SettingValue.Self => game.GameFolderPath,
            _ => game.Owner.FolderPath,
        };
    }
    #endregion
        
}