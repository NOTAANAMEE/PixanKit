using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.ModController.Mod;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using PixanKit.ModController.ModReader;

namespace PixanKit.ModController.Module
{
    /// <summary>
    /// Represents a mod collection. This class helps control the mod under
    /// the mod directory.
    /// </summary>
    public class ModCollection: IToJSON
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
            ModCache = cache;
            foreach (var mod in Directory.GetFiles(Owner.ModDir))
            {
                var modfile = ModReader.ModReader.ReadFile(this, mod);
                ModFiles.Add(modfile.MetaData.ModID, modfile);
            }
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
        /// Register the mod with its path and JSON data 
        /// </summary>
        /// <param name="path">The path of the mod file</param>
        /// <param name="jsonData">The JSON data of the mod file<br/>
        /// <code>
        /// {
        ///     "id": "",//The ID of the mod
        ///     "release_date": "",//The release date of the mod
        ///     "version": "",//The version of the mod
        ///     "depends": ["id"]//The array of the mod id of the dependencies
        /// }</code>
        /// </param>
        public void Register(string path, JObject jsonData)
        {
            if (!path.StartsWith(Owner.ModDir)) return;
            var tmp = ModReader.ModReader.Open(path);
            string id = jsonData["id"]?.ToString() ?? "";
            ModFile file = JsonModReader.LoadFromFile(this,
                jsonData, tmp, id);
            ModFiles.Add(id, file);
            ModReader.ModReader.Free(tmp);
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
            ModFile file = ModReader.ModReader.ReadFile(this, path);
            ModFiles.Add(file.MetaData.ModID, file);
        }

        ///<inheritdoc/>
        public JObject ToJSON()
        {
            JObject jsonData = [];
            foreach (var item in ModFiles)
                jsonData.Add(item.Key, item.Value.ToJSON());
            return jsonData;
        }
    }
}
