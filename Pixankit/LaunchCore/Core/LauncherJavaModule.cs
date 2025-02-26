using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.JavaModule.Java;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Gets the collection of Java runtimes added to the launcher.
        /// </summary>
        public JavaRuntime[] JavaRuntimes
        {
            get => [.. _javaRuntimes];
        }

        List<JavaRuntime> _javaRuntimes = [];

        /// <summary>
        /// Adds a Java runtime to the launcher.
        /// </summary>
        /// <param name="runtime">A valid <see cref="JavaRuntime"/> instance.</param>
        public void AddJavaRuntime(JavaRuntime runtime)
        {
            _javaRuntimes.Add(runtime);
        }

        /// <summary>
        /// Adds a Java runtime to the launcher by specifying its installation path.
        /// </summary>
        /// <param name="path">The folder path of the Java runtime installation.
        /// For example: "C:\\Program Files\\Java\\jdk-21"</param>
        public void AddJavaRuntime(string path)
        {
            AddJavaRuntime(new JavaRuntime(path));
        }

        /// <summary>
        /// Removes a Java runtime from the launcher.
        /// </summary>
        /// <param name="runtime">A valid <see cref="JavaRuntime"/> instance that is already added to the launcher.</param>
        public void RemoveJavaRuntime(JavaRuntime runtime) 
        {
            _javaRuntimes.Remove(runtime);
        }

        /// <summary>
        /// Chooses an appropriate Java runtime for a specified Minecraft game.
        /// </summary>
        /// <param name="game">The <see cref="GameBase"/> instance representing the Minecraft game.</param>
        /// <returns>A <see cref="JavaRuntime"/> instance that is selected based on the game's settings, or <c>null</c> if no runtime matches.</returns>
        public JavaRuntime? ChooseRuntime(GameBase game)
        {
            JavaRuntime? runtime;
            switch (game.Settings["java"]?.ToString() ?? "overall")
            {
                case "overall":
                    game.Settings["java"] = Settings["java"];
                    runtime = ChooseRuntime(game);
                    game.Settings["java"] = JToken.FromObject("overall");
                    break;
                case "specified":
                    runtime = JavaChooser.Specified(JavaRuntimes, game);
                    break;
                case "closest":
                    runtime = JavaChooser.Closest(JavaRuntimes, game);
                    break;
                case "newest":
                    runtime = JavaChooser.Newest(JavaRuntimes, game);
                    break;
                default:
                    runtime = JavaRuntimes[0];
                    break;
            }
            return runtime;
        }
    }
}
