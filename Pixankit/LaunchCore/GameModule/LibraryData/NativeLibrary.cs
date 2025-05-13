using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.SystemInf;
using System.IO.Compression;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Represents a native library used in the Minecraft environment.
    /// </summary>
    public class NativeLibrary : LibraryBase
    {
        private string[] Exclude = [];

        /// <summary>
        /// Gets the path of the library.
        /// </summary>
        public override string LibraryPath => "${library_directory}" + Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibrary"/> class with the specified JSON data.
        /// </summary>
        /// <param name="libraryJData">The JSON data representing the native library.</param>
        public NativeLibrary(JObject libraryJData) : base()
        {
            libraryType = LibraryType.Native;
            string OSKey =
                libraryJData.GetOrDefault(Format.ToString,
                $"natives/{SysInfo.OSName}", "");

            JObject current = libraryJData.GetValue(Format.ToJObject, $"downloads/classifiers/{OSKey}");
            _name = current.GetValue(Format.ToString, "path");
            _sha1 = current.GetValue(Format.ToString, "sha1");
            _url = current.GetValue(Format.ToString, "url");

            List<string> excludelist = [];
            if (libraryJData.TryGetValue(Format.ToJArray, "extract/exclude", out JArray? array))
            {
                foreach (JToken token in array ?? [])
                    excludelist.Add(token.ToString());
            }
            Exclude = [.. excludelist];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibrary"/> class for internal use without extraction.
        /// </summary>
        protected internal NativeLibrary() : base()
        {
            libraryType = LibraryType.Native;
        }

        /// <summary>
        /// Extracts the files in the native library's JAR file to the specified directory.
        /// </summary>
        /// <param name="librarypath">The library directory</param>
        /// <param name="nativesPath">The directory to extract files to.</param>
        public void Extract(string librarypath, string nativesPath)
        {
            FileStream fs = new(LibraryPath.Replace("${library_directory}", librarypath + '/'), FileMode.Open);
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
            foreach (string path in Exclude)
            {
                if (fullPath.StartsWith(path)) return false;
            }
            return true;
        }

        private void Decompress(ZipArchiveEntry entry, string nativesdirpath)
        {
            if (entry.FullName.EndsWith('/')) return;
            string fullPath = $"{nativesdirpath}/{entry.FullName}";
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? "./");
            if (File.Exists(fullPath))
                Logger.Info($"{fullPath} Already exists, canceled");
            else
                entry.ExtractToFile(fullPath);
            Logger.Info($"Finish decompressing {fullPath}");
        }
    }
}
