using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.JavaModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.Extention;

namespace PixanKit.LaunchCore.Core
{
    public class JavaManager
    {
        #region PropertiesAndFields
        /// <summary>
        /// Gets the collection of Java runtimes added to the launcher.
        /// </summary>
        public IReadOnlyList<JavaRuntime> JavaRuntimes => _javaRuntimes.AsReadOnly();

        private List<JavaRuntime> _javaRuntimes = [];
        #endregion

        #region Initor
        internal JavaManager()
        {
            InitJavaModule();
        }

        private void InitJavaModule()
        {
            List<JavaRuntime> javaRuntimes = [];
            foreach (JToken jData in Files.RuntimeJData["children"] ?? new JObject())
            {
                javaRuntimes.Add(new JavaRuntime((JObject)jData));
            }
            _javaRuntimes = javaRuntimes;
        }
        #endregion

        #region Methods
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
            var javaSetting = game.Settings["java"]?.ToString();

            if (string.IsNullOrEmpty(javaSetting) || javaSetting == "overall")
            {
                javaSetting = Launcher.Instance.Settings["java"]?.ToString() ?? "default";
            }

            return javaSetting switch
            {
                "specified" => JavaChooser.Specified(_javaRuntimes, game),
                "closest"   => JavaChooser.Closest(JavaRuntimes, game),
                "newest"    => JavaChooser.Newest(JavaRuntimes, game),
                _           => JavaRuntimes.FirstOrDefault(),
            };
        }

        internal JObject Save()
        {
            var javaRuntimes = JavaRuntimes.Select(r => r.ToJSON());
            return new JObject()
            {
                { "children", JArray.FromObject(javaRuntimes)},
            };
        }
        #endregion
    }
}
