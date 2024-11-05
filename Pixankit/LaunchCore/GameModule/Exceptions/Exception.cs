using PixanKit.LaunchCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Exceptions
{
    /// <summary>
    /// Library/Token Does Not Support Your System
    /// </summary>
    public class SystemNotSupportedException : Exception
    {
        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="aimedsystem">Aimed System</param>
        /// <param name="currentsystem">The Current System</param>
        public SystemNotSupportedException(string aimedsystem, string currentsystem) : base("The System Is Not Supported")
        {
            SupportedSystem = aimedsystem;
            CurrentSystem = currentsystem;
            Logger.Error(Message);
        }

        /// <summary>
        /// Supported System
        /// </summary>
        public string SupportedSystem { get; private set; }

        /// <summary>
        /// Current System
        /// </summary>
        public string CurrentSystem { get; private set; }
    }
}
