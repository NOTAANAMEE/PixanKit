using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.GameModule.Mod;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// Minecraft Game With Mod Loader
    /// </summary>
    public class ModloaderGame : ModifiedGame
    {
        /// <summary>
        /// The mods in the folder
        /// </summary>
        public ModFile?[] Mods
        {
            get => _mods.Values.ToArray();
        }

        /// <summary>
        /// The Mod path. For instance: C:\Users\Admin\AppData\.minecraft\versions\1.12.2-Forge\mods
        /// </summary>
        public string ModDir
        {
            get => _path + "/mods";
        }

        /// <summary>
        /// Mod Dictionary.
        /// </summary>
        protected Dictionary<string, ModFile?> _mods = new();

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        /// <param name="jData"><inheritdoc/></param>
        public ModloaderGame(string path, JObject jData):base(path, jData) { }

        /// <summary>
        /// This method adds a modfile to the modloader game
        /// </summary>
        /// <param name="file"></param>
        public void AddMod(ModFile file)
        {
            if (file.ModInformation != null && Owner.Owner.FindMod(file.ID) == null)
                Owner.Owner.AddMod(file.ModInformation);
            _mods.Add(file.ID, file);
        }

        /// <summary>
        /// Recursively check the dependencies.
        /// </summary>
        /// <returns>The Missing Project ID</returns>
        public string[] CheckMissingDependencies()
        {
            List<string> list = new();
            Dictionary<string, ModFile?> _modsCopy = new(_mods);
            if (_modsCopy == null) return list.ToArray();
            foreach (var mod in _modsCopy)
            {
                if (mod.Value == null) continue;
                string[] tmp = mod.Value.CheckMissingDependenciesR(_modsCopy, true);
                var set = new HashSet<string>(list);
                set.UnionWith(tmp);
                list = set.ToList();
            }
            return list.ToArray();
        }

        /// <summary>
        /// Check whether the dependencies exist
        /// </summary>
        /// <returns>true if </returns>
        public bool CheckDependencies()
        {
            string[] tmp = CheckMissingDependencies();
            return tmp.Length == 0;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="DependencyException"><inheritdoc/></exception>
        public override bool LaunchCheck()
        {
            if (!CheckDependencies())
            {
                string[] tmp = CheckMissingDependencies();
                throw new DependencyException("Some of the mods need dependency libraries. That might cause crashes or unexpected results",
                    tmp);
            }
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="folder"><inheritdoc/></param>
        public override void SetOwner(Folder folder)
        {
            base.SetOwner(folder);
            InitMod();
        }

        private void InitMod()
        {
            if (_mods.Count > 0) return;
            JArray? mods = Initors.GetModList(this);
            Dictionary<string, string> files = GetFilesAndSha1();
            mods ??= new();
            foreach (JObject modJData in mods)
            {
                ModFile mod = new(this, modJData);
                mods.Add(mod);
                if (files.ContainsKey(mod.SHA1))
                {
                    mod._name = files[mod.SHA1];
                    mod.Status = ModStatus.Regular;
                    files.Remove(mod.SHA1);
                }
                else if (files.ContainsValue(mod.Name))
                {
                    mod.Status = ModStatus.ContentChanged;
                    RemoveDictionary(files, mod.Name);
                }
                else mod.Status = ModStatus.NotFound;
            }
            ImportMods(files);
            if (mods.Count != 0)mods.Remove();
        }

        private Dictionary<string, string> GetFilesAndSha1()
        {
            if (!Directory.Exists(Localize.PathLocalize(ModDir))) return new Dictionary<string, string>();
            string[] files = Directory.GetFiles(Localize.PathLocalize(ModDir));
            Dictionary<string, string> ret = new();
            foreach (string file in files) 
            {
                ret.Add(Files.GetSha1(file), System.IO.Path.GetFileName(file));
            }
            return ret;
        }

        private static void RemoveDictionary(Dictionary<string, string> dict, string value) 
        {
            foreach(KeyValuePair<string, string> kvp in dict)
            {
                if (kvp.Key == value) 
                    dict.Remove(kvp.Key);
            }
        }

        private void ImportMods(Dictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                _mods.Add(kvp.Value, new ModFile(ModDir + '/' + kvp.Value));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public JArray ModToJSON()
        {
            JArray mods = new();
            foreach (var mod in _mods)
            {
                mods.Add(mod.Value.ToJSON());
            }
            return mods;
        }
    }
}
