using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.JavaModule.Java
{
    public partial class JavaRuntime
    {
        /// <inheritdoc/>
        public void LoadFromJSON(JObject obj)
        {
            _javaFolder = obj["path"]?.ToString() ?? "";
            _version = (ushort)(obj["version"]?.ToObject<ushort>() ?? 0);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public JObject ToJSON()
        {
            return new JObject()
            {
                { "path", _javaFolder },
                { "version", _version },
            };
        }
    }
}
