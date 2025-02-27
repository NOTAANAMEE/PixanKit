using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Represents the base class for managing libraries in a Minecraft environment.
    /// </summary>
    /// <remarks>
    /// This abstract class defines the structure and behavior of various library types, 
    /// such as Vanilla libraries, native libraries, and mod libraries. 
    /// It includes functionality for initializing libraries from JSON data, 
    /// determining their type and compatibility, and managing their paths and URLs.
    /// </remarks>
    public abstract class LibraryBase
    {
        #region StaticFields
        static Dictionary<string, LibraryBase> libraries = [];

        /// <summary>
        /// Parses the library according to the JSON data
        /// </summary>
        /// <param name="jData">the JSON data of the object</param>
        /// <param name="gamelibraries">The list of the library</param>
        public static void Parse(JObject jData, List<LibraryBase> gamelibraries)
        {
            if (!SystemSupport(jData)) return;

            if (libraries.TryGetValue(GetName(jData), out LibraryBase? ret))
            {
                gamelibraries.Add(ret);
                return;
            }
            LibraryType type = GetLibraryType(jData);
            JObject? nativesData = jData["natives"] as JObject;
            JObject? downloadsData = jData["downloads"] as JObject;
            switch (type)
            {
                case LibraryType.Vanilla:
                    gamelibraries.Add(new OriginalLibrary(jData));
                    break;
                case LibraryType.Native:
                    if (nativesData?[SysInfo.OSName] == null) return;
                    gamelibraries.Add(new NativeLibrary(jData));
                    if (downloadsData != null && downloadsData.Count > 1)
                        gamelibraries.Add(new OriginalLibrary(jData));
                    break;
                case LibraryType.Mod:
                    gamelibraries.Add(new LoaderLibrary(jData));
                    break;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Library Name Like <c>com.ibm.icu:icu4j:73.2</c>
        /// </summary>
        public string Name { get => _name; }

        /// <summary>
        /// The Absolute Path Of The Library
        /// </summary>
        public string LibraryPath { get => "${library_directory}" + GetPath(_name); }

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
        #endregion

        #region Fields
        /// <summary>
        /// The Reference Count
        /// </summary>
        public int ReferenceCount = 0;

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
        protected LibraryType libraryType = LibraryType.Vanilla;
        #endregion

        #region Initors
        /// <summary>
        /// Initor Init the library
        /// </summary>
        /// <param name="jData"></param>
        /// <param name="libraryPath">The Library Folder Path</param>
        /// <exception cref="SystemNotSupportedException"></exception>
        public LibraryBase(JObject jData, string libraryPath)
        {
            //test System
            var os = GetAllowedSystem(jData);
            //Output Error
            if (!os.Contains(SysInfo.OSName)) 
                throw new SystemNotSupportedException(string.Join(',', os), SysInfo.OSName);
            _name = jData.GetOrDefault(JSON.Format.ToString, "name", "");
        }

        /// <summary>
        /// Initor
        /// </summary>
        protected LibraryBase() { }
        #endregion

        #region Methods
        /// <summary>
        /// Checks whether the system suits for the argument or library
        /// </summary>
        /// <param name="libraryToken"></param>
        /// <returns></returns>
        public static bool SystemSupport(JObject libraryToken)
            => GetAllowedSystem(libraryToken).Contains(SysInfo.OSName);

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
        public static string[] GetAllowedSystem(JObject jData)
        {
            // 默认允许所有操作系统
            if (jData["rules"] == null) return ["osx", "linux", "windows"];

            HashSet<string> OSSet = ["osx", "linux", "windows"];

            foreach (JToken ruleData in jData.GetOrDefault(JSON.Format.ToJArray, "rules", []))
            {
                JObject jObj = ruleData.ConvertTo(JSON.Format.ToJObject, []);

                string action = jObj.GetOrDefault(JSON.Format.ToString, "action", "");
                var osData = jObj.GetOrDefault(JSON.Format.ToJObject, "os", []);
                var osName = jObj.GetOrDefault(JSON.Format.ToString, "os/name", null);
                var osArch = jObj.GetOrDefault(JSON.Format.ToString, "os/arch", null);

                switch (action)
                {
                case "allow":
                    if (osData == null) OSSet = [ "osx", "linux", "windows" ];
                    else if (osName != null) OSSet.Add(osName);

                    if (osArch != null && osArch != SysInfo.CPUArch) return [];
                    break;

                case "disallow":
                    if (osName != null) OSSet.Remove(osName);
                    break;
                }
            }

            return [.. OSSet];
        }

        /// <summary>
        /// This is for judging which library type the library is
        /// </summary>
        /// <param name="jData"></param>
        /// <returns></returns>
        public static LibraryType GetLibraryType(JToken jData)
        {
            if (jData["natives"] != null) return LibraryType.Native;
            if (jData["downloads"] != null) return LibraryType.Vanilla;
            return LibraryType.Mod;
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

        private static string GetName(JObject jData)
            => (jData["name"]?? "").ToString();
        #endregion
    }

    /// <summary>
    /// Several library types
    /// </summary>
    public enum LibraryType
    {
        /// <summary>
        /// Vanilla Library Type. Just Download
        /// </summary>
        Vanilla,
        /// <summary>
        /// Native Library Type. Need Extract
        /// </summary>
        Native,
        /// <summary>
        /// Mod Loader Generated. Just download
        /// </summary>
        Mod
    }
}
