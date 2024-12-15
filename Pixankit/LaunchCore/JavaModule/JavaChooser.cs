using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.SystemInf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.JavaModule
{
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
        public static JavaRuntime? Specified(JavaRuntime[] runtimes, GameBase game)
        {
            foreach (JavaRuntime runtime in runtimes) 
            {
                if (runtime.Version == game.javaVersion)return runtime;
            }
            return null;
        }

        /// <summary>
        /// Choose The Runtime That Is Close To The Version That JSON Specified.<br/>
        /// The Chosen Runtime >= The Specified Version
        /// </summary>
        /// <param name="runtimes">Java Runtime Collection</param>
        /// <param name="game">Game Needed To Launch</param>
        /// <returns>Returns A JavaRuntime If Exisits. Else Return null</returns>
        public static JavaRuntime? Closest(JavaRuntime[] runtimes, GameBase game)
        {
            List<JavaRuntime> runtimes_ = runtimes.ToList();
            runtimes_.Sort((x, y) =>
            {
                return x.Version - y.Version;
            });
            foreach (JavaRuntime runtime in runtimes_) 
            {
                if (runtime.Version >= game.javaVersion) return runtime;
            }
            return null;
        }

        /// <summary>
        /// Choose The Newest Runtime
        /// </summary>
        /// <param name="runtimes">Java Runtime Collection</param>
        /// <param name="game">Game Needed To Launch</param>
        /// <returns>Returns A JavaRuntime If Exisits. Else Return null</returns>
        public static JavaRuntime? Newest(JavaRuntime[] runtimes, GameBase game)
        {
            return Newest(runtimes);
        }

        /// <summary>
        /// Choose The Newest
        /// </summary>
        /// <param name="runtimes">Java Runtime Array</param>
        /// <returns>Newest Java Runtime</returns>
        public static JavaRuntime? Newest(JavaRuntime[] runtimes)
        {
            List<JavaRuntime> runtimes_ = runtimes.ToList();
            runtimes_.Sort((x, y) =>
            {
                return x.Version - y.Version;
            });
            return runtimes_[^1];
        }
    }


}
