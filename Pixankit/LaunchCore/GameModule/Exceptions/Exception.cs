using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Exceptions
{
    public class SystemNotSupportedException : Exception
    {
        public SystemNotSupportedException(string aimedsystem, string currentsystem) : base("The System Is Not Supported")
        {
            SupportedSystem = aimedsystem;
            CurrentSystem = currentsystem;
        }

        public string SupportedSystem { get; private set; }

        public string CurrentSystem { get; private set; }
    }
}
