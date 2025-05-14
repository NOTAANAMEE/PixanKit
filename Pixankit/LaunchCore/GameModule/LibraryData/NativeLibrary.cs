using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.SystemInf;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Represents a native library used in the Minecraft environment.
    /// </summary>
    public class NativeLibrary : LibraryBase
    {
        #region Fields
        private string[] _exclude = [];

        /// <summary>
        /// Gets the path of the library.
        /// </summary>
        public override string LibraryPath => "${library_directory}" + Name;
        #endregion

        #region Init
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibrary"/> class for internal use without extraction.
        /// </summary>
        private NativeLibrary() : base()
        {
            LibraryType = LibraryData.LibraryType.Native;
        }
        #endregion

        #region Factory

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraryJData"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static bool CreateInstance(JObject libraryJData, [NotNullWhen(true)]out LibraryBase? library)
        {
            library = null;
            string osKey =
                libraryJData.GetOrDefault(Format.ToString,
                    $"natives/{SysInfo.OsName}", "null");
            if (osKey == "null") return false;
            
            JObject current = libraryJData.GetValue(Format.ToJObject, $"downloads/classifiers/{osKey}");
            string path = current.GetValue(Format.ToString, "path");
            string sha1 = current.GetValue(Format.ToString, "sha1");
            string url = current.GetValue(Format.ToString, "url");
            
            List<string> excludelist = [];
            if (libraryJData.TryGetValue(Format.ToJArray, "extract/exclude", out JArray? array))
            {
                excludelist.AddRange(from token in array ?? [] select token.ToString());
            }

            library = new NativeLibrary()
            {
                Name = path,
                Sha1 = sha1,
                Url = url,
                _exclude = [.. excludelist],
                LibraryType = LibraryData.LibraryType.Native
            };
            return true;
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Extracts the files in the native library's JAR file to the specified directory.
        /// </summary>
        /// <param name="libraryPath">The library directory</param>
        /// <param name="nativesPath">The directory to extract files to.</param>
        public void Extract(string libraryPath, string nativesPath)
        {
            FileStream fs = new(LibraryPath.Replace("${library_directory}", libraryPath + '/'), FileMode.Open);
            ZipArchive archive = new(fs);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!NeedsDecompress(entry.FullName))
                    continue;
                Decompress(entry, nativesPath);
            }
            fs.Close();
        }

        /// <summary>
        /// Determines whether the specified file path should be excluded from extraction.
        /// </summary>
        /// <param name="fullPath">The full path of the file.</param>
        /// <returns><c>true</c> if the file should be included; otherwise, <c>false</c>.</returns>
        private bool NeedsDecompress(string fullPath)
        {
            return _exclude.All(path => !fullPath.StartsWith(path));
        }

        private static void Decompress(ZipArchiveEntry entry, string nativesDirPath)
        {
            if (entry.FullName.EndsWith('/')) return;
            string fullPath = $"{nativesDirPath}/{entry.FullName}";
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? "./");
            if (File.Exists(fullPath))
                Logger.Logger.Info($"{fullPath} Already exists, canceled");
            else
                entry.ExtractToFile(fullPath);
            Logger.Logger.Info($"Finish decompressing {fullPath}");
        }
        #endregion
    }
}
