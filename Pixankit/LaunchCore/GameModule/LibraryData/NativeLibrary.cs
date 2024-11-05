using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Native Library Type. This Kind Of Library Need To Extract The Files In Jar
    /// </summary>
    public class NativeLibrary:LibraryBase
    {
        private string[] Exclude = Array.Empty<string>();

        bool NotExtract = false;

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="libraryJData"></param>
        public NativeLibrary(JToken libraryJData) : base()
        {
            libraryType = LibraryType.Native;
            /*if (libraryJData["natives"] == null)
            {
                _name = libraryJData["name"].ToString();
                _sha1 = libraryJData["downloads"]["artifact"]["sha1"].ToString();
                _url = libraryJData["downloads"]["artifact"]["url"].ToString();
                Console.WriteLine(Path);
                return;
            }*/
            JToken? current = libraryJData["downloads"]["classifiers"][libraryJData["natives"][SystemInformation.OSName].ToString()];
            _name = current["path"].ToString();
            _sha1 = current["sha1"].ToString();
            _url = current["url"].ToString();
            List<string> excludelist = new();
            if (libraryJData["extract"] != null && libraryJData["extract"]["exclude"] != null)
            {
                foreach (JToken token in libraryJData["extract"]["exclude"])
                    excludelist.Add(token.ToString());
            }
            Exclude = excludelist.ToArray();
        }

        /// <summary>
        /// Nothing. Just For Fun
        /// </summary>
        protected internal NativeLibrary():base(){ libraryType = LibraryType.Native; NotExtract = true; }

        /// <summary>
        /// Extract The Files In The Jar File
        /// </summary>
        /// <param name="nativesPath">Directory That Files Needed To Be Extracted To</param>
        public void Extract(string nativesPath)
        {
            if (NotExtract) return;
            FileStream fs = new(libraryPath + "/" +  Path, FileMode.Open);
            ZipArchive archive = new(fs);
            foreach (ZipArchiveEntry entry in archive.Entries) 
            {
                string fullPath = $"{nativesPath}/{entry.FullName}";
                
                
                if (!Judge(entry.FullName))
                    continue;
                Console.WriteLine($"Decompressing {fullPath}");
                if (fullPath.EndsWith("/"))
                {
                    fullPath = Localize.PathLocalize(fullPath);
                    Directory.CreateDirectory(fullPath);
                    continue;
                }
                string fullDir = Localize.PathLocalize(fullPath.Remove(fullPath.LastIndexOf('/')));
                fullPath = Localize.PathLocalize(fullPath);
                if (!Directory.Exists(fullDir))
                    Directory.CreateDirectory(fullDir);
                if (File.Exists(fullPath))
                    Console.WriteLine($"{fullPath} Already exists, canceled");
                
                else entry.ExtractToFile($"{fullPath}");
                Console.WriteLine($"Finish compressing {fullPath}");
            }
            fs.Close();
        }

        private bool Judge(string fullPath)
        {
            foreach (string path in Exclude)
            {
                if (fullPath.StartsWith(path)) return false;
            }
            return true; 
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override NativeLibrary Copy()
        {
            return new NativeLibrary()
            {
                Exclude = Exclude,
                _name = _name,
                _url = _url,
                _sha1 = _sha1,
            };
        }
    }
}
