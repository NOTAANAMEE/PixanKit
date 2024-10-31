using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Mod
{
    public class ModFile:IToJSON
    {
        public ModBase? ModInformation;

        /// <summary>
        /// The FileName
        /// </summary>
        public string Name
        {
            get => _name;
        }

        /// <summary>
        /// The Actual Path
        /// </summary>
        public string Path
        {
            get => (modloaderGame == null) ? _name : modloaderGame.ModDir + '/' + _name;
        }

        /// <summary>
        /// SHA1 Indicator
        /// </summary>
        public string SHA1 
        { 
            get => _sha1; 
        }

        /// <summary>
        /// The dependency of the mod
        /// </summary>
        public string[] Dependencies
        {
            get => _dependencies.ToArray();
        }

        public List<ModType> SupportModLoaders = new();

        public DateTime PublishedTime;

        internal string ID = "";

        internal string _name = "";

        protected string _sha1 = "";

        protected ModloaderGame? modloaderGame;

        protected List<string> _dependencies = new();

        public ModStatus Status;

        public ModFile(JObject jData)
        {
            _name = jData["path"].ToString();
            _sha1 = jData["sha1"].ToString();
            ID = jData["id"].ToString();
            PublishedTime = DateTime.Parse(jData["published"].ToString());
        }

        public ModFile(ModloaderGame game, JObject jData)
        {
            modloaderGame = game;
            _name = jData["path"].ToString();
            _sha1 = jData["sha1"].ToString();
            ID = jData["id"].ToString();
            foreach (JToken token in jData["modloader"]) SupportModLoaders.Add((ModType)(int)token);
            foreach (JToken token in jData["dependencies"])_dependencies.Add(token.ToString());
            PublishedTime = DateTime.Parse(jData["published"].ToString());
        }

        public ModFile(string path)
        {
            _name = path;
            _sha1 = Files.GetSha1(Path);
        }

        /// <summary>
        /// Check whether the dependencies exist
        /// </summary>
        /// <param name="mods">The mods</param>
        /// <param name="type">
        /// 0:Strict Check, check whether the file valid as well
        /// 1:Normal Check, check whether the dependencies exist
        /// 2:Do not Check, return true
        /// </param>
        /// <returns>true:go ahead and launch false:check the dependencies</returns>
        public bool CheckDependencies(Dictionary<string, ModFile?> mods, ushort type)
        {
            bool result = true;
            foreach (string tmp in _dependencies) 
            {
                switch (type)
                {
                    case 0:
                        result = result && mods.ContainsKey(tmp) && mods[tmp] != null && mods[tmp].Status <= ModStatus.ContentChanged;
                        break;
                    case 1:
                        result = result && mods.ContainsKey(tmp) && mods[tmp] != null && mods[tmp].Status <= ModStatus.ContentChanged;
                        break;
                    case 2:
                        return true;
                }
            }
            return result;
        }

        private string[] CheckMissingDependencies(Dictionary<string, ModFile?> mods)
        {
            List<string> result = new List<string>();
            foreach (string tmp in _dependencies)
            {
                if (!mods.ContainsKey(tmp)) result.Add(tmp);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mods">This Method Will Change The Value Of The Dictionary! Copy The Dictionary!</param>
        /// <param name="recursively"></param>
        /// <returns></returns>
        public string[] CheckMissingDependenciesR(Dictionary<string, ModFile?> mods, bool recursively)
        {
            if (!recursively) return CheckMissingDependencies(mods);
            List<string> result = new();
            foreach (string tmp in _dependencies)
            {
                if (!mods.ContainsKey(tmp)) result.Add(tmp);
                else if (mods[tmp] != null)
                {
                    result.AddRange(mods[tmp].CheckMissingDependenciesR(mods, recursively));
                    mods[tmp] = null;
                }
            }
            return result.ToArray();
        }

        private void Init()
        {
            if (modloaderGame != null && modloaderGame.Owner != null && modloaderGame.Owner.Owner != null) 
            {
                ModInformation = modloaderGame.Owner.Owner.FindMod(ID);
            }
        }

        public JObject ToJSON()
        {
            JObject jobj = new()
            {
                { "path", _name },
                { "sha1", _sha1 },
                { "publish", PublishedTime.ToString() },
                { "id", ID },
                { "modloader", new JArray(SupportModLoaders.Select((e)=>(int)e).ToList()) }
            };
            return jobj;
        }
    }

    public enum ModStatus
    {
        Regular, //The same file name, The same SHA1
        NameChanged, //Not the same name, But the same SHA1
        ContentChanged, //The same name, But not the same SHA1
        NotFound//Not found
    }

    public class DependencyException : Exception
    {
        public DependencyException(string message, string[] needmods) : base(message)
        {
            NeedMods = needmods;
        }

        public readonly string[] NeedMods;
    }
}
