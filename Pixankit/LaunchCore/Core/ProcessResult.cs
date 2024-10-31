using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Core
{
    /// <summary>
    /// The result of Minecraft Process
    /// </summary>
    public struct ProcessResult
    {
        /// <summary>
        /// The Return Code
        /// </summary>
        public int ReturnCode;

        /// <summary>
        /// Normal Exit?
        /// </summary>
        public bool Successful { get => ReturnCode == 1; }

        /// <summary>
        /// Log File Path. .tar.gz file
        /// </summary>
        public string LogGZPath;

        /// <summary>
        /// The Crash File Path
        /// </summary>
        public string? CrashFilePath;

        /// <summary>
        /// The Output Stream
        /// </summary>
        public Stream OutputStream;

        /// <summary>
        /// Close the OutputStream
        /// </summary>
        public void Close()
        {
            OutputStream.Close();
        }
    }
}
