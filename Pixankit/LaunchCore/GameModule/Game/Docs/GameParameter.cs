using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.Game;

public partial class GameParameter
{
    /// <summary>
    /// Gets the JVM Version
    /// </summary>
    public partial short JvmVersion { get; private set; }
    
    /// <summary>
    /// Gets the game type.
    /// </summary>
    public partial bool IsModified { get; }
    
    /// <summary>
    /// Decide whether the arguments should be based on Vanilla game arguments
    /// </summary>
    public partial bool ReliedArgs { get; }
    
    /// <summary>
    /// Gets the Java arguments.
    /// </summary>
    public partial string JavaArgs { get; }
    
    /// <summary>
    /// Gets the game arguments.
    /// </summary>
    public partial string GameArgs { get; }
    
    /// <summary>
    /// Gets the entry class
    /// </summary>
    public partial string MainClass { get; }
    
    /// <summary>
    /// Gets the Assets ID of a game
    /// </summary>
    public partial string AssetsId { get; }
    
    /// <summary>
    ///  Judge the game is original or modified
    /// </summary>
    /// <param name="jData">the jsonData of the game</param>
    /// <param name="isModified">
    /// Explanation:
    /// 0: It is the original game arguments, not modified.
    /// 1: It is modified by mod loaders and the args are incomplete
    /// 2: It is modified by mod loaders but the args are complete
    /// </param>
    /// <returns>the based version of the game</returns>
    public static partial string GetVersion(JObject jData, out int isModified);

    /// <summary>
    /// Create an instance of the GameParameter
    /// </summary>
    /// <param name="jData"></param>
    /// <returns></returns>
    public static partial GameParameter CreateInstance(JObject jData);
}