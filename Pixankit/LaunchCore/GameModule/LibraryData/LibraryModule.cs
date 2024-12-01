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
using System.Reflection;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// This is the base class of Library Class.
    /// </summary>
    public abstract class LibraryBase
    {
        /// <summary>
        /// Library Name Like <c>com.ibm.icu:icu4j:73.2</c>
        /// </summary>
        public string Name { get => _name; }

        /// <summary>
        /// The Absolute Path Of The Library
        /// </summary>
        public string Path { get => libraryPath + GetPath(_name); }

        /// <summary>
        /// Download URL Of The Library
        /// </summary>
        public string Url { get => _url; }

        /// <summary>
        /// The Type Of The Library
        /// </summary>
        public LibraryType LibraryType { get => libraryType; }

        /// <summary>
        /// SHA1 Of The Folder
        /// </summary>
        public string SHA1 { get => _sha1; }

        /// <summary>
        /// The Reference Count
        /// </summary>
        public int ReferenceCount = 0;

        /// <summary>
        /// Library Directory
        /// </summary>
        protected string libraryPath = "";

        /// <summary>
        /// Name
        /// </summary>
        protected string _name = "";

        /// <summary>
        /// URL
        /// </summary>
        protected string _url = "";

        /// <summary>
        /// SHA1
        /// </summary>
        protected string _sha1 = "";

        /// <summary>
        /// Type
        /// </summary>
        protected LibraryType libraryType = LibraryType.Original;

        /// <summary>
        /// Initor Init the library
        /// </summary>
        /// <param name="jData"></param>
        /// <param name="libraryPath">The Library Folder Path</param>
        /// <exception cref="SystemNotSupportedException"></exception>
        public LibraryBase(JToken jData, string libraryPath)
        {
            //test System
            var os = GetAllowedSystem(jData);
            //Output Error
            if (!os.Contains(SystemInformation.OSName)) throw new SystemNotSupportedException(string.Join(',', os), SystemInformation.OSName);
            _name = (jData["name"]?? "").ToString();
            this.libraryPath = libraryPath;
        }

        /// <summary>
        /// Initor
        /// </summary>
        protected LibraryBase() { }

        /// <summary>
        /// This is for judging which system is suitable for this library
        /// </summary>
        /// <param name="jData">The Library Json Data. Like <br/><c>
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
        ///},</c></param>
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
        /// <param name="jData"></param>
        /// <returns></returns>
        public static LibraryType GetLibraryType(JToken jData)
        {
            if (jData["natives"] != null) return LibraryType.Native;
            if (jData["downloads"] != null) return LibraryType.Original;
            return LibraryType.Mod;
        }

        /// <summary>
        /// Set The Library Directory Of A Library
        /// </summary>
        /// <param name="folder">The Target Folder</param>
        public void SetFolder(Folder folder)
        {
            libraryPath = folder.LibraryDir;
        }

        /// <summary>
        /// Get The Path Of The Library
        /// </summary>
        /// <param name="name">Name Like <c>"com.mojang:logging:1.4.9"</c></param>
        /// <returns>Path Of The Library. Like 
        /// <c>"/com/mojang/logging/1.4.9/logging-1.4.9.jar"</c></returns>
        protected static string GetPath(string name)
        {
            if (name.Contains('/')) return name;
            string pathInf = name;
            string path = "";
            string[] pathInfs = pathInf.Split(":");
            pathInfs[^1] = pathInfs[^1].Replace(".jar", "");
            if (pathInfs.Length == 3) path = $"/{pathInfs[0].Replace('.', '/')}/{pathInfs[1]}/{pathInfs[2]}/{pathInfs[1]}-{pathInfs[2]}.jar";
            if (pathInfs.Length == 4) path = $"/{pathInfs[0].Replace('.', '/')}/{pathInfs[1]}/{pathInfs[2]}/{pathInfs[1]}-{pathInfs[2]}-{pathInfs[3]}.jar";
            return path;
        }

        /// <summary>
        /// Copy A New LibraryBase Instance
        /// </summary>
        /// <returns>The Copied Instance</returns>
        public abstract LibraryBase Copy();
    }

    /// <summary>
    /// It contains 
    /// </summary>
    public enum LibraryType
    {
        /// <summary>
        /// Original Library Type. Just Download
        /// </summary>
        Original,
        /// <summary>
        /// Native Library Type. Need Extract
        /// </summary>
        Native,
        /// <summary>
        /// Mod Loader Generated. Need To Do Nothing With It
        /// </summary>
        Mod
    }
}
