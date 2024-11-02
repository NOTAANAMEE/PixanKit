using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Json
{
    /// <summary>
    /// An interface to store the data
    /// </summary>
    public interface IToJSON
    {
        /// <summary>
        /// Store The Data By JSON
        /// </summary>
        /// <returns>The JSON Object</returns>
        public JObject ToJSON(); 
    }
}
