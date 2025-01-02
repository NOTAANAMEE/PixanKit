using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace PixanKit.ModModule.Exceptions
{
    public class TOMLKeyException(TomlTable table, string key, string message) : Exception(message)
    {
        public readonly TomlTable Table = table;

        public readonly string Key = key;
    }
}
