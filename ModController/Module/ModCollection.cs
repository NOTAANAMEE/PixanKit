using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Log;
using PixanKit.ModController.Mod;
using PixanKit.ModController.ModReader;

namespace PixanKit.ModController.Module
{
    /// <summary>
    /// Represents a mod collection. This class helps control the mod under
    /// the mod directory.
    /// </summary>
    public partial class ModCollection: IToJSON
    {
        /// <summary>
        /// The modded game of the mods
        /// </summary>
        public ModdedGame Owner;

        /// <summary>
        /// The mods under the mod directory<br/>
        /// <c>string</c>: The ID of the mod<br/>
        /// <c>ModFile</c>: The <see cref="ModFile"/> which represent mods
        /// </summary>
        public Dictionary<string, ModFile> ModFiles = [];

        /// <summary>
        /// The JSON cache of each mod. <c>key</c> is the ID
        /// and <c>value</c> is the JSON cache of each mod file
        /// </summary>
        internal JObject ModCache = [];

        /// <summary>
        /// Inits the <see cref="ModCollection"/> through the cache and the 
        /// <see cref="Owner"/> of the collection.
        /// </summary>
        /// <param name="cache">The Cache of the game</param>
        /// <param name="game"><see cref="Owner"/> of the collection</param>
        public ModCollection(JObject cache, ModdedGame game)
        {
            Owner = game;
            LoadFromJSON(cache);
            foreach (var mod in Directory.GetFiles(Owner.ModDir))
            {
                try { 
                var modfile = ModParser.Parse(mod, this);
                ModFiles.TryAdd(modfile.MetaData?.ModID ?? "", modfile); }
                catch(Exception e) 
                { 
                    Logger.Error("ModController", $"Error while parsing {mod}: {e.Message}"); 
                    Logger.Error("ModController", e.StackTrace ?? "");
                }
            }
            ModCache = [];
        }

        /// <summary>
        /// Inits the <see cref="ModCollection"/> through the 
        /// <see cref="Owner"/> of the collection.<br/>
        /// The cache will be automatically set as <c>new JObject()</c>
        /// </summary>
        /// <param name="game"><see cref="Owner"/> of the collection</param>
        public ModCollection(ModdedGame game):this([], game) { }

        /// <summary>
        /// This method gets mandatory dependencies that are needed for existing mods
        /// </summary>
        /// <returns>The list of the mod ID</returns>
        public List<string> GetDependencies()
        {
            List<string> dependencies = [];
            foreach (ModFile modFile in ModFiles.Values)
                modFile.GetDependencies(dependencies);
            return dependencies;
        }

        /// <summary>
        /// This method checks if all the dependencies are satisfied
        /// </summary>
        /// <returns><c>true</c> if no other dependencies needed</returns>
        public bool CheckDependencies()
        {
            var dependencies = GetDependencies();
            foreach (var dependency in dependencies)
                if (!ModFiles.ContainsKey(dependency)) return false;
            return true;
        }

        /// <summary>
        /// Register the mod with its path.
        /// The data of the mod will be read from its file.
        /// </summary>
        /// <param name="path">
        /// The path of the mod file
        /// </param>
        public void Register(string path)
        {
            if (!path.StartsWith(Owner.ModDir)) return;
            ModFile file = ModParser.Parse(path, this);
            if (file.MetaData == null) throw new Exception("WTF");
            ModFiles.Add(file.MetaData.ModID, file);
        }

        
    }
}
