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
    /// Represents a native library used in the Minecraft environment.
    /// </summary>
    public class NativeLibrary : LibraryBase
    {
        private string[] Exclude = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibrary"/> class with the specified JSON data.
        /// </summary>
        /// <param name="libraryJData">The JSON data representing the native library.</param>
        public NativeLibrary(JToken libraryJData) : base()
        {
            libraryType = LibraryType.Native;
            JToken? current = libraryJData["downloads"]?["classifiers"]?
                [libraryJData["natives"]?[SystemInformation.OSName]?.ToString() ?? ""];
            _name = current?["path"]?.ToString() ?? "";
            _sha1 = current?["sha1"]?.ToString() ?? "";
            _url = current?["url"]?.ToString() ?? "";
            List<string> excludelist = [];
            if (libraryJData["extract"] != null && libraryJData["extract"]?["exclude"] != null)
            {
                foreach (JToken token in libraryJData["extract"]?["exclude"] ?? new JArray())
                    excludelist.Add(token.ToString());
            }
            Exclude = [..excludelist];
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
            FileStream fs = new(librarypath + "/" + Path, FileMode.Open);
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
                else
                    entry.ExtractToFile(fullPath);
                Console.WriteLine($"Finish decompressing {fullPath}");
            }
            fs.Close();
        }

        /// <summary>
        /// Determines whether the specified file path should be excluded from extraction.
        /// </summary>
        /// <param name="fullPath">The full path of the file.</param>
        /// <returns><c>true</c> if the file should be included; otherwise, <c>false</c>.</returns>
        private bool Judge(string fullPath)
        {
            foreach (string path in Exclude)
            {
                if (fullPath.StartsWith(path)) return false;
            }
            return true;
        }
    }
}
