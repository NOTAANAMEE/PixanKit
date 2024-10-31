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
    public class OrdinaryLibrary: LibraryBase
    {
        public OrdinaryLibrary(JToken libraryJData):base(libraryJData) 
        {
            libraryType = LibraryType.Ordinary;
            _url = libraryJData["downloads"]["artifact"]["url"].ToString();
            _sha1 = libraryJData["downloads"]["artifact"]["sha1"].ToString();
        }

        protected OrdinaryLibrary():base()
        {
            libraryType = LibraryType.Ordinary;
        }

        public OrdinaryLibrary Copy()
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
