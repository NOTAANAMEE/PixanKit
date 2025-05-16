using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.Json;
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
        public LaunchSession.LaunchSession Launch(GameBase game,PlayerBase player)
        {
            var cmd = InlineCommand(game, player);
            Logger.Logger.Info("Game Arg Generated Successfully.");

            JavaRuntime java = JavaManager.ChooseRuntime(game) ?? throw new NullReferenceException();

            game.Decompress().Wait();

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
            game.LaunchCheck();
            string cmd = game.GetLaunchArgument();

            cmd = PlayerInLine(cmd, player);
            cmd = $"-Xmx{Initers.GetMemory()}m " + cmd;
            cmd = cmd.Replace("${launcher_name}", LauncherName);
            cmd = cmd.Replace("${launcher_version}", VersionName);
            cmd = cmd.Replace("${game_directory", GetGameRunningFolder(game));
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
            var tmp = game.Settings["env_variables"]?.ToString() ?? "";
            var env = tmp switch
            {
                "custom" => game.
                     Settings.GetOrDefault(Format.ToJObject, "custom_env_variables",
                    new JObject()),
                _ => Settings.GetOrDefault(Format.ToJObject, "env_variables",
                    new JObject()),
            };
            return env.Properties()
                .Select(
                    a => 
                        new KeyValuePair<string, string>(
                            a.Name, a.Value.ToString()))
                .ToList();
        }
        
        private string GetPreArguments(GameBase game)
        {
            var postArg = game.Settings["post_argument"]?.ToString();
            return postArg switch
            {
                "overall" => Settings.GetOrDefault(Format.ToString, "post_argument", ""),
                _ => postArg ?? "",
            };
        }

        private string GetPostArguments(GameBase game)
        {
            var postArg = game.Settings["post_argument"]?.ToString();
            return postArg switch
            {
                "overall" => Settings.GetOrDefault(Format.ToString, "post_argument", ""),
                _ => postArg ?? "",
            };
        }

        private string GetJvmArguments(GameBase game)
        {
            var jvmArg = game.Settings["jvm_argument"]?.ToString();
            return jvmArg switch
            {
                "custom" => game.Settings.GetOrDefault(Format.ToString, "custom_jvm_argument", ""),
                _ => Settings.GetOrDefault(Format.ToString, "jvm_argument", ""),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public string GetGameRunningFolder(GameBase game)
        {
            var gameDir = game.Settings.
                GetOrDefault(Format.ToString, "running_folder", "overall");
            switch (gameDir)
            {
                case "custom":
                    return game.Settings.GetOrDefault(
                        Format.ToString, "custom_running_folder", gameDir);
                case "overall":
                    gameDir = Settings.
                        GetOrDefault(Format.ToString, "running_folder", "self");
                    break;
            }
            return gameDir switch
            {
                "self" => game.GameFolderPath,
                _ => Settings.GetOrDefault(Format.ToString, 
                    "custom_running_folder", gameDir)
            };
        }
        #endregion
        
    }
}
