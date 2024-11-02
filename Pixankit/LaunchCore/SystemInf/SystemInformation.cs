using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.SystemInf
{
    /// <summary>
    /// Provides Information Like Operating System And Computer Architecture
    /// </summary>
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
        /// <br/>
        /// x86: x86<br/>
        /// x86_64: x86_64, AMD64<br/>
        /// arm64: Arm64 AArch64
        /// </summary>
        public static string CPUArch = "x86";
    }
}
