using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Exceptions
{
    public class NoFolderException:Exception
    {
        public NoFolderException(string message) : base(message) { }

        public NoFolderException() { }
    }
}
