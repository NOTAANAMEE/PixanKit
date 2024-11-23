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
    public class ModInf
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public string Icon { get; set; } = "";

        public string ID { get; set; } = "";

        public string[] Authors { get; set; } = Array.Empty<string>();

        internal int Ref = 0;

        protected ModInf(TomlTable tomldoc) 
        {
            if (tomldoc["mods"] is not TomlTable mod) throw new Exception();
            Name = mod["displayName"] as string ?? "";
            Description = mod["description"] as string ?? "";
            Icon = mod["logoFile"] as string ?? "";
            ID = mod["modId"] as string ?? "";
            Authors = (mod["authors"] as string ?? "").Split(',');
        }

        protected ModInf(JObject jsondoc)
        {
            Name = (jsondoc["id"] ?? "").ToString();
            Description = (jsondoc["description"] ?? "").ToString();
            Icon = (jsondoc["icon"] ?? "").ToString();
            ID = (jsondoc["id"] ?? "").ToString();
            Authors = (jsondoc["authors"] as JArray ?? new JArray()).Select(x => x.ToString()).ToArray();
        }

        public ModInf() { }

        #region StaticConfigLoad
        public static ModInf Load(TomlTable tomldoc)
        {
            return new(tomldoc);
        }

        public static ModInf Load(JObject jsondoc)
        {
            return new(jsondoc);
        }
        #endregion

        public JObject ToJSON()
        {
            return new()
            {
                { "name", Name
                },
                { "description", Description
                },
                { "id", ID
                },
                { "icon", Icon
                },
                { "authors", new JArray(Authors)
                },
            };
        }

    }
}
