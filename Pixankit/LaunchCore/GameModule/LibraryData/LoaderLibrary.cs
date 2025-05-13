using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;

namespace PixanKit.LaunchCore.GameModule.LibraryData
{
    /// <summary>
    /// Represents a library specific to mod loaders.
    /// </summary>
    public class LoaderLibrary : LibraryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoaderLibrary"/> class with a specified folder and library JSON data.
        /// </summary>
        /// <param name="folder">The directory where the library is located.</param>
        /// <param name="libraryJData">The JSON data representing the library.</param>
        public LoaderLibrary(string folder, JObject libraryJData) :
            base(libraryJData, folder)
        {
            libraryType = LibraryType.Mod;
            try
            {
                _url = libraryJData.GetValue(Format.ToString, "url");
            }
            catch
            {
                _url = "";
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoaderLibrary"/> class with library JSON data.
        /// </summary>
        /// <param name="libraryJData">The JSON data representing the library.</param>
        public LoaderLibrary(JObject libraryJData) : this("", libraryJData) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoaderLibrary"/> class for internal use.
        /// </summary>
        protected LoaderLibrary() : base()
        {
            libraryType = LibraryType.Mod;
        }
    }
}
