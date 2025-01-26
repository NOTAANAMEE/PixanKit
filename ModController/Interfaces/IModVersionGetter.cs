using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModController.Interfaces
{
    public interface IModVersionGetter
    {
        public Task<JArray> GetVersionsAsync(CancellationToken token);
    }
}
