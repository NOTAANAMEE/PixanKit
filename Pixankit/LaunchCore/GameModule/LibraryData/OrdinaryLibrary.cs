using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Ordinary Kind Of Library
    /// </summary>
    public class OrdinaryLibrary: LibraryBase
    {
        /// <summary>
        /// Initor Of Ordinary Library
        /// </summary>
        /// <param name="folder">The Library Directory</param>
        /// <param name="libraryJData">The JSON Data Of Library</param>
        public OrdinaryLibrary(string folder, JToken libraryJData):base(libraryJData, folder) 
        {
            libraryType = LibraryType.Ordinary;
            _url = libraryJData["downloads"]["artifact"]["url"].ToString();
            _sha1 = libraryJData["downloads"]["artifact"]["sha1"].ToString();
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="libraryJData"></param>
        public OrdinaryLibrary(JToken libraryJData):this("", libraryJData) { }

        /// <summary>
        /// Initor For Copy
        /// </summary>
        protected OrdinaryLibrary():base()
        {
            libraryType = LibraryType.Ordinary;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override LibraryBase Copy()
        {
            return new OrdinaryLibrary()
            {
                _name = _name,
                _url = _url,
                _sha1 = _sha1,
            };
        }
    }
}
