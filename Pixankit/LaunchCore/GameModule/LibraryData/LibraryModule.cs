using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// This is the base class of Library Class.
    /// </summary>
    public class LibraryBase
    {
        public string Name { get => _name; }

        public string Path { get => GetPath(_name); }

        public string Url { get => _url; }

        public LibraryType LibraryType { get => libraryType; }

        public string SHA1 { get => _sha1; }

        public int ReferenceCount = 0;

        protected string _name = "";

        //protected string _path = "";

        protected string _url = "";

        protected string _sha1 = "";

        protected LibraryType libraryType = LibraryType.Ordinary;

        /// <summary>
        /// Initor Init the library
        /// </summary>
        /// <param name="jData"></param>
        /// <exception cref="SystemNotSupportedException"></exception>
        public LibraryBase(JToken jData)
        {
            //test System
            var os = GetAllowedSystem(jData);
            //Output Error
            if (!os.Contains(SystemInformation.OSName)) throw new SystemNotSupportedException(string.Join(',', os), SystemInformation.OSName);
            _name = jData["name"].ToString();
        }

        protected LibraryBase() { }

        /// <summary>
        /// This is for judging which system is suitable for this library
        /// </summary>
        /// <param name="libraryJData">The Library Json Data. Like 
        /// {
        ///"downloads": {
        /// "artifact": {
        ///  "path": "org/slf4j/slf4j-api/2.0.9/slf4j-api-2.0.9.jar",
        ///  "sha1": "7cf2726fdcfbc8610f9a71fb3ed639871f315340",
        ///  "size": 64579,
        ///  "url": "https://libraries.minecraft.net/org/slf4j/slf4j-api/2.0.9/slf4j-api-2.0.9.jar"
        /// }
        /// },
        ///"name":"org:slf4j:slf4j-api:2.0.9"
        ///},</param>
        /// <returns></returns>
        public static string[] GetAllowedSystem(JToken jData)
        {
            if (jData["rules"] == null) return new string[] { "osx", "linux", "windows" };
            List<string> OSSet = new();
            foreach (JToken ruleData in jData["rules"])
            {
                if (ruleData["action"].ToString() == "allow")
                {
                    if (ruleData["os"] == null) OSSet = new List<string> { "osx", "linux", "windows" };
                    else if (ruleData["os"]["name"] != null) OSSet.Add(ruleData["os"]["name"].ToString());

                    if (ruleData["os"] == null || ruleData["os"]["arch"] == null) continue;
                    if (ruleData["os"]["arch"].ToString() == SystemInformation.CPUArch) continue;
                    return Array.Empty<string>();
                }
                else
                {
                    OSSet.Remove(ruleData["os"].ToString());
                }
            }
            return OSSet.ToArray();
        }

        /// <summary>
        /// This is for judging which library type the library is
        /// </summary>
        /// <param name="libraryJData"></param>
        /// <returns></returns>
        public static LibraryType GetLibraryType(JToken jData)
        {
            if (jData["natives"] != null) return LibraryType.Native;
            if (jData["downloads"] != null) return LibraryType.Ordinary;
            return LibraryType.Mod;
        } 

        /*private string NameParse(string name)
        {
            string[] str = name.Split(':');
            string tmp = str[0];
            if (str.Length > 3) tmp += '.' + str[1];
            str[^1] = str[^1].Replace(".jar", "");
            return $"{(tmp).Replace('.', '/')}/{str[^2]}/{str[^1]}/{str[^2]}-{str[^1]}.jar";
        }*/

        protected static string GetPath(string name)
        {
            if (name.Contains('/')) return name;
            string pathInf = name;
            string path = "";
            string[] pathInfs = pathInf.Split(":");
            pathInfs[^1] = pathInfs[^1].Replace(".jar", "");
            if (pathInfs.Length == 3) path = $"{pathInfs[0].Replace('.', '/')}/{pathInfs[1]}/{pathInfs[2]}/{pathInfs[1]}-{pathInfs[2]}.jar";
            if (pathInfs.Length == 4) path = $"{pathInfs[0].Replace('.', '/')}/{pathInfs[1]}/{pathInfs[2]}/{pathInfs[1]}-{pathInfs[2]}-{pathInfs[3]}.jar";
            return path;
        }
    }

    /// <summary>
    /// It contains 
    /// </summary>
    public enum LibraryType
    {
        Ordinary,
        Native,
        Mod
    }
}
