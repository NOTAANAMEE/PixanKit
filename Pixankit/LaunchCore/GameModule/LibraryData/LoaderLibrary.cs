using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Library That ModLoader Installer Generated
    /// </summary>
    public class LoaderLibrary: LibraryBase
    {
        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="folder">The Library Dir</param>
        /// <param name="libraryJData">The JSON Data Of A Library</param>
        public LoaderLibrary(string folder, JToken libraryJData):base(libraryJData, folder) 
        { 
            libraryType = LibraryType.Mod;
            if (libraryJData["url"] != null) _url = libraryJData["url"].ToString();
        }

        /// <summary>
        /// Initor. Please Set The Folder
        /// </summary>
        /// <param name="libraryJData"></param>
        public LoaderLibrary(JToken libraryJData):this("", libraryJData) { }

        /// <summary>
        /// For Copy
        /// </summary>
        protected LoaderLibrary():base() { libraryType = LibraryType.Mod; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override LoaderLibrary Copy()
        {
            return new LoaderLibrary()
            {
                _name = _name,
                _url = _url,
                _sha1 = _sha1,
            };

        }
    }
}
