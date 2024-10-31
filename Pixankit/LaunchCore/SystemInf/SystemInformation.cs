using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.SystemInf
{
    public static class SystemInformation
    {
        /// <summary>
        /// The system name. 
        /// windows linux osx unix
        /// windows: Just Windows
        /// linux: Ubuntu, Debian, Arch, ChromeOS...
        /// osx: OSX, MacOS 11-Latest
        /// unix: others
        /// </summary>
        public static string OSName = "windows";

        /// <summary>
        /// System Arch
        /// x86 x86_64 arm64
        /// x86: x86
        /// x86_64: x86_64, AMD64
        /// arm64: Arm64 AArch64
        /// </summary>
        public static string CPUArch = "x86";
    }
}
