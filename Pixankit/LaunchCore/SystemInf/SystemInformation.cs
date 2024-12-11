using PixanKit.LaunchCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.SystemInf
{
    /// <summary>
    /// Provides Information Like Operating System And Computer Architecture
    /// </summary>
    public static class SystemInformation
    {
        [ModuleInitializer]
        internal static void Init()
        {
            SetOS();
            SetArch();
        }

        private static void SetOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) OSName = "windows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) OSName = "linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) OSName = "osx";
            else OSName = "others";
        }

        private static void SetArch()
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    CPUArch = "x86";
                    break;
                case Architecture.X64:
                    CPUArch = "x86_64";
                    break;
                case Architecture.Arm:
                    CPUArch = "arm";
                    break;
                case Architecture.Arm64:
                    CPUArch = "arm64";
                    break;
                /*case Architecture.LoongArch64://Just For Fun
                default:
                    Console.WriteLine("Fuck You");
                    throw new Exception("Fuck You");*/
                default:
                    Logger.Warn("The Program And Minecraft Is Not Tested On Your CPU Arch, " +
                        "You Might Lose Your Gaming Data");
                    break;
            }
        }

        /// <summary>
        /// Represents the name of the operating system<br/>
        /// windows: Windows7 -> Latest Windows<br/>
        /// linux: Ubuntu, Debian, Arch, ChromeOS, Fedora, RedHat...<br/>
        /// osx: OSX, MacOS 11 -> Latest MacOS<br/>
        /// unix: others<br/>
        /// </summary>
        public static string OSName = "windows";

        /// <summary>
        /// Represents the architecture of the cpu
        /// <br/>
        /// x86: x86 i386<br/>
        /// x86_64: x86_64, AMD64<br/>
        /// arm64: Arm64 AArch64 Arm_v8
        /// arm: Arm A32
        /// </summary>
        public static string CPUArch = "x86_64";
    }
}
