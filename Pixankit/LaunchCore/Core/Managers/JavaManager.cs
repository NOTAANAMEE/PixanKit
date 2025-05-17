using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.JavaModule.Java;

namespace PixanKit.LaunchCore.Core.Managers;

/// <summary>
/// The java manager instance of the launcher.
/// </summary>
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
        foreach (var jData in Files.RuntimeJData["children"] ?? new JObject())
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
        var javaSetting = game.Settings.JavaSetting;

        if (javaSetting == SettingValue.Overall)
        {
            javaSetting = Launcher.Instance.Setting.JavaSetting;
        }

        return javaSetting switch
        {
            SettingValue.Specified => JavaChooser.Specified(_javaRuntimes, game),
            SettingValue.Closest   => JavaChooser.Closest(JavaRuntimes, game),
            _                      => JavaChooser.Newest(JavaRuntimes, game),
        };
    }

    internal JObject Save()
    {
        var javaRuntimes = JavaRuntimes.Select(r => r.ToJson());
        return new JObject()
        {
            { "children", JArray.FromObject(javaRuntimes)},
        };
    }
    #endregion
}