using PixanKit.LaunchCore.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Mod
{
    /// <summary>
    /// Stores Mod information. ModFile will refer the instance of this class
    /// </summary>
    public class ModBase: IToJSON
    {
        /// <summary>
        /// Name of the mod
        /// </summary>
        public string Name
        {
            get => _name;
        }

        /// <summary>
        /// Curse Forge and Modrinth
        /// </summary>
        public string ID
        {
            get => _id;
        }

        /// <summary>
        /// The website of the mod. Not the Curse Forge/Modrinth
        /// </summary>
        public string Website
        {
            get => _website;
        }

        /// <summary>
        /// The main page of the mod on Curse Forge.
        /// Tips: Do not directly crawl the content from any website
        /// </summary>
        public string CurseForgeURL
        {
            get => (Import)? "":"https://www.curseforge.com/minecraft/mc-mods/" + ID;
        }

        /// <summary>
        /// The main page of the mod on Curse Forge.
        /// Tips: Do not directly crawl the content from any website
        /// </summary>
        public string ModrinthURL
        {
            get => (Import)? "":"https://www.modrinth.com/mod/" + ID;
        }

        /// <summary>
        /// The description of the mod
        /// </summary>
        public string Description
        {
            get => _description;
        }

        /// <summary>
        /// The URI of the Icon
        /// </summary>
        public string IconURI
        {
            get => _iconuri;
        }

        /// <summary>
        /// Whether the mod is user imported or downloaded
        /// </summary>
        public bool Import
        {
            get => _import;
        }

        public bool NotUsing
        {
            get => referenceCount == 0;
        }

        /// <summary>
        /// The author of the mod
        /// </summary>
        public string[] Authors
        {
            get => _authors.ToArray();
        }

        //public DateTime LastModifiedDay;

        //public long DownloadCount;

        protected string _website = "";

        protected string _id = "";

        protected string _name = "";

        protected string _description = "";

        protected string _iconuri = "";

        protected bool _import = false;

        protected ushort referenceCount = 0;

        protected List<string> _authors = new();

        /// <summary>
        /// {
        /// "name":"",
        /// "id":"",
        /// "description":"",
        /// "icon":"",
        /// "import":false,
        /// "website":"",
        /// "modloader":
        ///     [
        ///         1,
        ///         2
        ///     ],
        /// ...others like file name and verification
        /// }
        /// </summary>
        /// <param name="jData"></param>
        public ModBase(JObject jData)
        {
            _name = jData["name"].ToString();
            _id = jData["id"].ToString();
            _description = jData["description"].ToString();
            _iconuri = jData["icon"].ToString();
            _import = (bool)jData["import"];
            _website = jData["website"].ToString();
            _authors = (jData["authors"] as JArray).Select(x => x.ToString()).ToList();
            
        }

        public ModBase() { }

        /// <summary>
        /// Set the information of modfile. referenceTime++
        /// </summary>
        /// <param name="file">The modfile instance</param>
        public void SetModInformation(ModFile file)
        {
            file.ModInformation = this;
            referenceCount++;
        }


        public virtual JObject ToJSON()
        {
            return new JObject()
            {
                { "name", _name },
                { "id", _id },
                { "description", _description },
                { "iconuri", _iconuri },
                { "import", Import },
                { "website", _website },
                { "authors", new JArray(Authors) },
            };
        }
    }

    public enum ModType
    {
        Forge,
        NeoForge,
        Fabric,
        Quilt,
        LiteLoader,
    }
}
