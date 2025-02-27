using Newtonsoft.Json.Linq;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Represents an Vanilla library in the Minecraft environment.
    /// </summary>
    public class OriginalLibrary : LibraryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalLibrary"/> class with a specified directory and library JSON data.
        /// </summary>
        /// <param name="folder">The directory where the library is located.</param>
        /// <param name="libraryJData">The JSON data representing the library.</param>
        public OriginalLibrary(string folder, JObject libraryJData) : base(libraryJData, folder)
        {
            libraryType = LibraryType.Vanilla;
            _url = libraryJData["downloads"]?["artifact"]?["url"]?.ToString() ?? "";
            _sha1 = libraryJData["downloads"]?["artifact"]?["sha1"]?.ToString() ?? "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalLibrary"/> class with library JSON data.
        /// </summary>
        /// <param name="libraryJData">The JSON data representing the library.</param>
        public OriginalLibrary(JObject libraryJData) : this("", libraryJData) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalLibrary"/> class for internal use.
        /// </summary>
        protected OriginalLibrary() : base()
        {
            libraryType = LibraryType.Vanilla;
        }
    }
}
