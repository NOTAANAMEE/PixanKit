using PixanKit.ModModule.Mods;
using PixanKit.LaunchCore.GameModule.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
/*Redesign
 * process:
 * 1.GetList Of Mod Files
 * 2.Load Cache Of Mod Inf And JSON Data
 * 3.For Each Mod Do:
 *  i.Get SHA1
 *  ii.Contains SHA1? If No, Try Load ID From Mod Config
 *  iii.Contains ID? If No, Try Load Data From Mod Config->iv
 *  iv.Add Mod Cache
 */
namespace PixanKit.ModModule.Module
{
    /// <summary>
    /// 
    /// </summary>
    public class ModCollection
    {
        public ModloaderGame Game;

        public ModModule Owner;

        public ModCollection(ModloaderGame game, ModModule owner)
        {
            Game = game;
            Owner = owner;
        }

        #region Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobj">
        /// <code>
        /// {
        ///  "files":{}
        /// }
        /// </code></param>
        public void SetCache(JObject jobj)
        {
            foreach (var item in jobj["files"] as JObject ?? new JObject()) 
            {
                JObject value = item.Value as JObject;
                string id = value["id"].ToString();
                ModFileSHACache.Add(item.Key, value);
                ModFileIDCache.Add(id, value.ToString());
                if (Owner.Mods.ContainsKey(id)) ModIDCache.Add(id, Owner.Mods[id]);
            }
        }

        private void LoadMods()
        {
            var files = Directory.GetFiles(Localize.PathLocalize(Game.ModDir));
            foreach (var file in files)
            {
                if (!file.EndsWith(".jar") && !file.EndsWith(".jar.disabled")) continue;
                ModFile? f = null;
                try
                {
                    f = new(file, this);
                }
                catch
                {
                    continue;
                }
                if (f != null) Mods.Add(f.ID, f);
            }
            ModFileSHACache.Clear();
            ModFileIDCache.Clear();
            ModIDCache.Clear();
        }
        #endregion

        #region Dicts
        private Dictionary<string, JObject> ModFileSHACache = new();

        private Dictionary<string, string> ModFileIDCache = new();

        private Dictionary<string, ModInf> ModIDCache = new();

        public Dictionary<string, ModFile> Mods = new();
        #endregion

        #region Cache
        internal void AddCache(ModInf inf)
        {
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

        public JObject ToJSON()
        {
            JObject JOb = new();
            foreach (var mod in Mods)
            {
                JOb.Add(mod.Value.SHA1, mod.Value.ToJSON());
            }
            return JOb;
        }
    }
}
