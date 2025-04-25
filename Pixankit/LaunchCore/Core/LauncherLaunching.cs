using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.PlayerModule.Player;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Launch the game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public LaunchSession Launch(GameBase game)
        {
            string cmd = InlineCommand(game);
            Logger.Info("Game Arg Generated Successfully. Stored in a.bat");

            JavaRuntime java = ChooseRuntime(game) ?? throw new NullReferenceException();

            game.Decompress().Wait();

            return new LaunchSession(game, java, cmd);
        }

        /// <summary>
        /// Launch the default game
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public LaunchSession Launch()
        {
            if (GameManager.Instance.TargetGame is null) throw new NullReferenceException();
            return Launch(GameManager.Instance.TargetGame);
        }

        private string InlineCommand(GameBase game)
        {
            game.LaunchCheck();
            long timeStamp = DateTime.Now.Ticks;
            string cmd = GenerateCommand(game);

            cmd = PlayerInLine(cmd);
            string pth = Path.GetDirectoryName(game.GameRootFolderPath) ?? "./";
            cmd = $"-Xmx{Initors.GetMemory()}m " + cmd;
            cmd = cmd.Replace("${launcher_name}", LauncherName);
            cmd = cmd.Replace("${launcher_version}", VersionName);
            return cmd;
        }

        /// <summary>
        /// Generate The Launch Command
        /// </summary>
        /// <param name="game">target gae</param>
        /// <returns>command</returns>
        public string GenerateCommand(GameBase game)
        {
            string cmd = game.GetLaunchArgument();
            cmd = cmd.Replace("${arguments}",
                Settings.GetOrDefault(Format.ToString, "arguments", ""));
            return cmd;
        }

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
            => PlayerInLine(arg, PlayerManager.Instance.TargetPlayer ??
                throw new NullReferenceException("Target player not found"));
    }
}
