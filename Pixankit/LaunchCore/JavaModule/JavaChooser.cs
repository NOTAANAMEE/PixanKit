using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.JavaModule
{
    public static class JavaChooser
    {
        public static JavaRuntime? Specified(JavaRuntime[] runtimes, GameBase game)
        {
            foreach (JavaRuntime runtime in runtimes) 
            {
                if (runtime.Version == game.javaVersion)return runtime;
            }
            return null;
        }

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

        public static JavaRuntime? Newest(JavaRuntime[] runtimes, GameBase game)
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
