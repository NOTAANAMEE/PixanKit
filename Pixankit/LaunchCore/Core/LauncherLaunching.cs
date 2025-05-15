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

        public LaunchSession.LaunchSession Launch(GameBase game,PlayerBase player)
        {
            string cmd = InlineCommand(game, player);
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
            string cmd = GenerateCommand(game);

            cmd = PlayerInLine(cmd, player);
            cmd = $"-Xmx{Initers.GetMemory()}m " + cmd;
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
            cmd = cmd.Replace("${jvm_argument}", GetJvmArguments(game));
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
            => PlayerInLine(arg, PlayerManager.TargetPlayer ??
                throw new NullReferenceException("Target player not found"));
        
        private List<KeyValuePair<string, string>> GetVariables(GameBase game)
        {
            if (game.Settings["env_variables"] is JObject tmp)
            {
                return tmp.Properties()
                    .Select(x => new KeyValuePair<string, string>(x.Name, x.Value.ToString()))
                    .ToList();
            }

            tmp = game.Settings["env_variables"] as JObject ?? new JObject();

            return tmp.Properties()
                .Select(x => new KeyValuePair<string,string>(x.Name, x.Value.ToString()))
                .ToList();
        }
        
        private string GetPreArguments(GameBase game)
        {
            var preArg = game.Settings["pre_argument"]?.ToString();
            if (preArg is null || preArg == "overall")
                preArg = Settings.GetOrDefault(
                    Format.ToString, "pre_argument", 
                    "");
            return preArg;
        }

        private string GetPostArguments(GameBase game)
        {
            var postArg = game.Settings["post_argument"]?.ToString();
            if (postArg is null || postArg == "overall")
                postArg = Settings.GetOrDefault(
                    Format.ToString, "post_argument", 
                    "");
            return postArg;
        }

        private string GetJvmArguments(GameBase game)
        {
            var jvmArg = game.Settings["jvm_argument"]?.ToString();
            if (jvmArg is null || jvmArg == "overall")
                jvmArg = Settings.GetOrDefault(
                    Format.ToString, "jvm_argument", 
                    "");
            return jvmArg;
        }
    }
}
