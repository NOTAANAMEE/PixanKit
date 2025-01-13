using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModModule.Interface
{
    public interface IModNewestGetter
    {
        public Task<string> GetLatestVersion(string modid, string mcversion);

    }
}
