using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Json
{
    /// <summary>
    /// Defines a method to convert an object to a JSON representation.
    /// </summary>
    public interface IToJSON
    {
        /// <summary>
        /// Converts the implementing object to a JSON object.
        /// </summary>
        /// <returns>A <see cref="JObject"/> representing the object's data.</returns>
        public JObject ToJSON();
    }
}
