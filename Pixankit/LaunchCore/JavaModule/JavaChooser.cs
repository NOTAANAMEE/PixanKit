using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;

namespace PixanKit.LaunchCore.JavaModule;

/// <summary>
/// Choose Specific Java With Settings
/// </summary>
public static class JavaChooser
{
    /// <summary>
    /// Choose The Runtime That Is The Same As The Version That JSON Specified
    /// </summary>
    /// <param name="runtimes">Java Runtime Collection</param>
    /// <param name="game">Game Needed To Launch</param>
    /// <returns>Returns A JavaRuntime If Exisits. Else Return null</returns>
    public static JavaRuntime? Specified(IEnumerable<JavaRuntime?> runtimes, GameBase game)
        => runtimes.FirstOrDefault(
            runtime => runtime?.Version == game.MinimalJavaVersion, 
            null);

    /// <summary>
    /// Choose The Runtime That Is Close To The Version That JSON Specified.<br/>
    /// The Chosen Runtime >= The Specified Version
    /// </summary>
    /// <param name="runtimes">Java Runtime Collection</param>
    /// <param name="game">Game Needed To Launch</param>
    /// <returns>Returns A JavaRuntime If Exisits. Else Return null</returns>
    public static JavaRuntime? Closest(IEnumerable<JavaRuntime> runtimes, GameBase game)
        => runtimes
            .Where(runtime => runtime.Version >= game.MinimalJavaVersion)
            .OrderBy(runtimes => runtimes.Version)
            .FirstOrDefault();

    /// <summary>
    /// This function will choose the latest JavaRuntime
    /// according to its major version
    /// </summary>
    /// <param name="runtimes">A collection of Java runtimes</param>
    /// <param name="game">The game that is needed to launch</param>
    /// <returns>
    /// The JavaRuntime instance which is in the collection and 
    /// matches the requirement.<br/>
    /// If no JavaRuntime fits, it will return <c>null</c>
    /// </returns>
    public static JavaRuntime? Newest(IEnumerable<JavaRuntime> runtimes, GameBase game)
        => Newest(runtimes);

    /// <summary>
    /// This function will choose the latest JavaRuntime
    /// according to its major version
    /// </summary>
    /// <param name="runtimes">A collection of Java runtimes</param>
    /// <returns>
    /// The JavaRuntime instance which is in the collection and 
    /// matches the requirement.<br/>
    /// If no JavaRuntime fits, it will return <c>null</c>
    /// </returns>
    public static JavaRuntime? Newest(IEnumerable<JavaRuntime> runtimes)
        => runtimes
            .OrderByDescending(runtime => runtime.Version)
            .FirstOrDefault();
}