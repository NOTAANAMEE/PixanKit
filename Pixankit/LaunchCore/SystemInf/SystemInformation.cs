using PixanKit.LaunchCore.Log;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixanKit.LaunchCore.SystemInf
{
    /// <summary>
    /// Provides Information Like Operating System And Computer Architecture
    /// </summary>
    public static class SysInfo
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
                default:
                    CPUArch = "other";
                    Logger.Warn("The Program And Minecraft might not be able to run on your PC" +
                        ", The game might crash");
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
        public static string OSName { get; private set; } = "windows";

        /// <summary>
        /// Represents the architecture of the cpu
        /// <br/>
        /// x86: x86 i386<br/>
        /// x86_64: x86_64, AMD64<br/>
        /// arm64: Arm64 AArch64 Arm_v8
        /// arm: Arm A32
        /// </summary>
        public static string CPUArch { get; private set; } = "x86_64";

        /// <summary>
        /// Gets the available memory size in megabytes.
        /// </summary>
        /// <returns>
        /// A long value representing the available memory size in megabytes.
        /// </returns>
        public static long GetAvailableMemSize()
        {
            return Environment.WorkingSet / 1024 / 1024;
        }
    }
}
