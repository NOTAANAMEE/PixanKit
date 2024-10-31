using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    public class LoaderLibrary: LibraryBase
    {
        public LoaderLibrary(JToken libraryJData):base(libraryJData) 
        { 
            libraryType = LibraryType.Mod;
            //string pathInf = libraryJData["name"].ToString();
            //string[] pathInfs = pathInf.Split(":");
            //_path = $"{pathInfs[^3].Replace('.', '/')}/{pathInfs[^2]}/{pathInfs[^1]}/{pathInfs[^2]}-{pathInfs[^1]}.jar";
            if (libraryJData["url"] != null) _url = libraryJData["url"].ToString();
            //if (pathInfs.Length == 4) _path = pathInfs[0].Replace('.', '/') + '/' + _path;
        }

        protected LoaderLibrary():base() { libraryType = LibraryType.Mod; }

        public LoaderLibrary Copy()
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
