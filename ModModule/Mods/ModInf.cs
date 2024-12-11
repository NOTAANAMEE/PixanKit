using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace PixanKit.ModModule.Mods
{
    /// <summary>
    /// Represents information about a mod, including its metadata such as name, description, icon, and authors.
    /// </summary>
    public class ModInf
    {
        /// <summary>
        /// Gets or sets the name of the mod.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the description of the mod.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the icon file path or URL of the mod.
        /// </summary>
        public string Icon { get; set; } = "";

        /// <summary>
        /// Gets or sets the unique identifier of the mod.
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Gets or sets the authors of the mod.
        /// </summary>
        public string[] Authors { get; set; } = [];

        internal int Ref = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModInf"/> class using data from a TOML document.
        /// </summary>
        /// <param name="tomldoc">The TOML document containing mod metadata.</param>
        /// <exception cref="Exception">Thrown if the TOML document does not contain valid mod metadata.</exception>
        protected ModInf(TomlTable tomldoc) 
        {
            if (tomldoc["mods"] is not TomlTableArray array) throw new Exception();
            var mod = array[0];
            Name = mod["displayName"] as string ?? "";
            Description = mod["description"] as string ?? "";
            if (mod.ContainsKey("logoFile")) Icon = mod["logoFile"] as string ?? "";
            ID = mod["modId"] as string ?? "";
            Authors = (mod["authors"] as string ?? "").Split(',');
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModInf"/> class using data from a JSON document.
        /// </summary>
        /// <param name="jsondoc">The JSON document containing mod metadata.</param>
        protected ModInf(JObject jsondoc)
        {
            Name = (jsondoc["id"] ?? "").ToString();
            Description = (jsondoc["description"] ?? "").ToString();
            Icon = (jsondoc["icon"] ?? "").ToString();
            ID = (jsondoc["id"] ?? "").ToString();
            Authors = (jsondoc["authors"] as JArray ?? new JArray()).Select(x => x.ToString()).ToArray();
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModInf"/> class.
        /// </summary>
        public ModInf() { }

        #region StaticConfigLoad
        /// <summary>
        /// Creates a new instance of <see cref="ModInf"/> by loading data from a TOML document.
        /// </summary>
        /// <param name="tomldoc">The TOML document containing mod metadata.</param>
        /// <returns>A new <see cref="ModInf"/> instance with the data loaded from the TOML document.</returns>
        public static ModInf Load(TomlTable tomldoc)
        {
            return new ModInf(tomldoc);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ModInf"/> by loading data from a JSON document.
        /// </summary>
        /// <param name="jsondoc">The JSON document containing mod metadata.</param>
        /// <returns>A new <see cref="ModInf"/> instance with the data loaded from the JSON document.</returns>
        public static ModInf Load(JObject jsondoc)
        {
            return new(jsondoc);
        }
        #endregion

        /// <summary>
        /// Converts the current instance of <see cref="ModInf"/> to a JSON object.
        /// </summary>
        /// <returns>A <see cref="JObject"/> representing the mod metadata.</returns>
        public JObject ToJSON()
        {
            return new()
            {
                { "name", Name },
                { "description", Description },
                { "id", ID },
                { "icon", Icon },
                { "authors", new JArray(Authors) },
            };
        }

    }
}
