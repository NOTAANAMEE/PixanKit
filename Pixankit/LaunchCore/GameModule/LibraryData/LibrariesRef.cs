using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// 
    /// </summary>
    public class LibrariesRef
    {
        /// <summary>
        /// 
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public LibraryBase[] Libraries => [.._libraries];
        
        private readonly List<LibraryBase> _libraries = new();

        private LibrariesRef(string version, JObject jData)
        {
            Version = version;
            JArray array = jData.GetOrDefault(Format.ToJArray, "libraries", []);
            foreach (JToken token in array)
            {
                LibraryHelper.AddLibrary(
                    token.ConvertTo(Format.ToJObject, []), _libraries);
            }
            Logger.Logger.Info($"Libraries Added. Number:{_libraries.Count}");
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="jData"></param>
        /// <returns></returns>
        public static LibrariesRef CreateInstance(string version, JObject jData)
            => new LibrariesRef(version, jData);
    }
}