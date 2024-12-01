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
    /// Original Kind Of Library
    /// </summary>
    public class OriginalLibrary: LibraryBase
    {
        /// <summary>
        /// Initor Of Original Library
        /// </summary>
        /// <param name="folder">The Library Directory</param>
        /// <param name="libraryJData">The JSON Data Of Library</param>
        public OriginalLibrary(string folder, JToken libraryJData):base(libraryJData, folder) 
        {
            libraryType = LibraryType.Original;
            _url = libraryJData["downloads"]["artifact"]["url"].ToString();
            _sha1 = libraryJData["downloads"]["artifact"]["sha1"].ToString();
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="libraryJData"></param>
        public OriginalLibrary(JToken libraryJData):this("", libraryJData) { }

        /// <summary>
        /// Initor For Copy
        /// </summary>
        protected OriginalLibrary():base()
        {
            libraryType = LibraryType.Original;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override LibraryBase Copy()
        {
            return new OriginalLibrary()
            {
                _name = _name,
                _url = _url,
                _sha1 = _sha1,
            };
        }
    }
}
