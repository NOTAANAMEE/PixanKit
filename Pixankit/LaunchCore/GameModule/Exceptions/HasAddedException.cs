using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Exceptions
{
    public class HasAddedException:Exception
    {
        public HasAddedException(string message) : base(message) { }

        public HasAddedException() { }
    }
}
