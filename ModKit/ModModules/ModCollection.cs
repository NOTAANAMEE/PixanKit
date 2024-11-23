using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Log;
using PixanKit.ModKit;
using PixanKit.ModKit.ModModules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ModKit.ModModules
{
    /// <summary>
    /// Mod Collection Of A Game 
    /// </summary>
    public class ModCollection
    {
        #region Fields
        /// <summary>
        /// The Mod Directory
        /// </summary>
        public string ModDir = "";

        /// <summary>
        /// Mods Dictionary. String Is The Mod ID
        /// </summary>
        public Dictionary<string, ModFile?> Mods = new();
        #endregion

        #region Initors
        public ModCollection(string modDir) 
        {
            ModDir = modDir;
            ModsLoad();
        }

        public ModCollection() { }

        public ModCollection(ModloaderGame game):this(game.ModDir) { }
        #endregion

        #region InitorMethods
        private void ModsLoad()
        {
            string[] files = Directory.GetFiles(Localize.PathLocalize(ModDir));
            foreach (var file in files) 
            {
                if (!IsMod(file)) continue;
                try
                {
                    LoadMod(file);
                }
                catch
                {
                    Logger.Warn("PixanKit.ModModule", $"File {file} Is Not A Mod But Located In A Mod Directory. Please Remove It");
                }
            }
            Logger.Info("PixanKit.ModModule", $"Finish Reading {ModDir}. Loaded {Mods.Count} Mods");
        }

        private bool IsMod(string filePath)
        {
            return filePath.EndsWith(".jar") || filePath.EndsWith(".jar.disabled");
        }

        private ModFile ModFileLoad(string file)
        {
            ModFile modFile = new(Localize.PathLocalize(file));
            if (!modFile.Valid) ModCacheLoad(modFile);
            else IconLoad(modFile);
            return modFile;
        }

        private void LoadMod(string filePath)
        {
            var tmp = ModFileLoad(filePath);
            Mods.Add(tmp.ID, tmp);
        }

        private static void ModCacheLoad(ModFile mod)
        {
            string sha1 = ModFile.GetSHA1(mod.Path);
            if ((ModModule.ModCache["mods"] as JObject).ContainsKey(sha1))
                mod.JSONLoad(ModModule.ModCache["mods"][sha1] as JObject);
            else Logger.Warn("PixanKit.ModModule", $"Could Not Find Mod Cache For {mod.Path}! This Is An Invalid Mod");
        }

        private void IconLoad(ModFile mod)
        {
            if ((ModModule.ModCache["icon"] as JObject).ContainsKey(mod.ID) &&
                File.Exists(ModModule.ModCache["icon"][mod.ID].ToString())) return;
            IconUpdate(mod);
        }

        /// <summary>
        /// Enforce Update The Image
        /// </summary>
        /// <param name="mod"></param>
        private static void IconUpdate(ModFile mod)
        {
            if ((ModModule.ModCache["icon"] as JObject).ContainsKey(mod.ID))
                (ModModule.ModCache["icon"] as JObject).Remove(mod.ID);
            (ModModule.ModCache["icon"] as JObject).Add(mod.ID, mod.ExtractIcon());
        }
        #endregion

        public void AddMod(string filePath)
        {
            var destination = Localize.PathLocalize(
                $"{ModDir}/{Path.GetFileName(filePath)}");
            File.Copy(Localize.PathLocalize(filePath), destination);
            var tmp = ModFileLoad(destination);
            if (tmp.Valid) IconUpdate(tmp);
        }

        public void RemoveMod(ModFile mod) 
        {
            File.Delete(mod.Path);
            Mods.Remove(mod.ID);
        }

        #region Dependencies
        public bool CheckDependencies() 
        {
            return GetMissingDependencies().Count == 0;
        }

        private List<string> MissingDependencies(string modid, Dictionary<string, ModFile?> mods)
        {
            var ret = new List<string>();
            var mod = mods[modid];
            if (mod == null) return ret;
            foreach (var dependency in mod.Dependencies)
            {
                if (dependency.Key == "minecraft") continue;
                if (Mods.ContainsKey(dependency.Key))
                {
                    var tmp = mods[dependency.Key];
                    mods[dependency.Key] = null;
                    if (tmp != null)ret.AddRange(MissingDependencies(tmp.ID, mods));
                }
                else
                {
                    ret.Add(dependency.Key);
                }
            }
            return ret;
        }

        public List<string> GetMissingDependencies() 
        {
            var mods = new Dictionary<string, ModFile?>(Mods);
            List<string> ret = new();
            foreach (var modid in mods.Keys) 
            {
                ret.AddRange(MissingDependencies(modid, mods));
            }
            return ret;
        }
        #endregion
    }
}
