using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Log;
using PixanKit.ModModule.Mods;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Exceptions;

namespace PixanKit.ModModule.Module
{
    /// <summary>
    /// Represents a collection of mods managed by a specific mod loader and owner.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ModCollection"/> class.
    /// </remarks>
    /// <param name="game">The game instance associated with the mod loader.</param>
    /// <param name="owner">The owner module managing this mod collection.</param>
    public class ModCollection(ModdedGame game, ModModule owner)
    {
        /// <summary>
        /// The game associated with this mod collection.
        /// </summary>
        public ModdedGame Game = game;

        /// <summary>
        /// The owner module of this mod collection.
        /// </summary>
        public ModModule Owner = owner;

        #region Init
        /// <summary>
        /// Sets the cache for mod files based on the given JSON object.
        /// </summary>
        /// <param name="jobj">
        /// A JSON object in the format:
        /// <code>
        /// {
        ///  "files": {}
        /// }
        /// </code>
        /// </param>
        public void SetCache(JObject jobj)
        {
            foreach (var item in jobj["files"] as JObject ?? []) 
            {
                JObject value = item.Value is JObject data ? data : 
                    throw new InvalidOperationException("JToken not JObject");
                string id = value["id"]?.ToString() ?? 
                    throw new JSONKeyException(value, "id", "Fabric JSON Mod Document");
                ModFileSHACache.Add(item.Key, value);
                ModFileIDCache.Add(id, value.ToString());
                if (Owner.Mods.TryGetValue(id, out ModInf? inf)) ModIDCache.Add(id, inf);
            }
            LoadMods();
        }

        private void LoadMods()
        {
            if (!Directory.Exists(Localize.PathLocalize(Game.ModDir)))
                Directory.CreateDirectory((Localize.PathLocalize(Game.ModDir)));
            var files = Directory.GetFiles(Localize.PathLocalize(Game.ModDir));
            foreach (var file in files)
            {
                Logger.Info("PixanKit.ModModule", $"Start Initing File {file}");
                if (!file.EndsWith(".jar") && !file.EndsWith(".jar.disabled")) continue;
                ModFile? modfile;
                try
                {
                    modfile = new(file, this);
                }
                catch
                {
                    continue;
                }
                if (modfile != null) Mods.Add(modfile.ID, modfile);
            }
            ModFileSHACache.Clear();
            ModFileIDCache.Clear();
            ModIDCache.Clear();
        }
        #endregion

        #region Dicts
        private Dictionary<string, JObject> ModFileSHACache = [];

        private Dictionary<string, string> ModFileIDCache = [];

        private Dictionary<string, ModInf> ModIDCache = [];

        /// <summary>
        /// Dictionary of mods loaded in this collection.
        /// </summary>
        public Dictionary<string, ModFile?> Mods = [];
        #endregion

        #region Cache
        internal void AddCache(ModInf inf)
        {
            if (ModFileIDCache.ContainsKey(inf.ID)) return;
            ModIDCache.Add(inf.ID, inf);
        }

        internal void RemoveCache(ModInf inf) 
        {
            ModIDCache.Remove(inf.ID);
        }

        internal void CleanCache(ModInf inf)
        {
            foreach (var id in ModIDCache.Keys.Except(Mods.Keys))ModIDCache.Remove(id);
        }
        #endregion

        #region Load
        internal bool LoadFromSHA1(ModFile file)
        {
            if (file.SHA1 == "") throw new Exception();
            if (!ModFileSHACache.ContainsKey(file.SHA1))return false;
            file.SetInf(GetModInf(file.SHA1));
            file.LoadDependencies(ModFileSHACache[file.SHA1]);
            return true;
        }

        internal bool LoadFromID(ModFile file)
        {
            if (file.ID == "") throw new Exception();
            if (!Mods.ContainsKey(file.ID)) return false;
            file.SetInf(ModIDCache[file.ID]);
            file.LoadDependencies(ModFileSHACache[ModFileIDCache[file.ID]]);
            return true;
        }

        internal bool TryLoadInfFromOwnerCache(ModFile file)
        {
            if (!Owner.Mods.ContainsKey(file.ID)) return false;
            file.SetInf(Owner.Mods[file.ID]);
            return true;
        }

        private ModInf GetModInf(string SHA1)
        {
            var id = (ModFileSHACache[SHA1]["id"]?? "").ToString();
            return ModIDCache[id];
        }
        #endregion

        #region Dependencies
        /// <summary>
        /// Checks whether all dependencies for loaded mods are satisfied.
        /// </summary>
        /// <returns>True if all dependencies are satisfied, otherwise false.</returns>
        public bool CheckDependencies()
        {
            return LostDependencies().Count == 0;
        }

        /// <summary>
        /// Retrieves a list of missing dependencies for the loaded mods.
        /// </summary>
        /// <returns>A list of missing dependency identifiers.</returns>
        public List<string> LostDependencies()
        {
            var copy = new Dictionary<string, ModFile?>(Mods);
            if (copy == null) return [];
            List<string> ret = [];
            foreach (var key in copy.Keys)
            {
                var mod = copy[key];
                if (mod == null) continue;
                ret.AddRange(mod.LostDependenciesR(copy));
            }
            ret.Remove("minecraft");
            ret.Remove("java");
            ret.Remove(Game.ModLoader);
            return ret;
        }
        #endregion

        /// <summary>
        /// Converts the current collection of mods to a JSON object.
        /// </summary>
        /// <returns>
        /// A <see cref="JObject"/> where each key is a mod's SHA1 hash 
        /// and the value is the mod's JSON representation.
        /// </returns>
        public JObject ToJSON()
        {
            JObject ret = [];
            foreach (var mod in Mods)
            {
                if (mod.Value != null)
                ret.Add(mod.Value.SHA1, mod.Value.ToJSON());
            }
            return ret;
        }
    }
}
