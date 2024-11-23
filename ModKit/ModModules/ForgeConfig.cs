using PixanKit.ModKit.ModModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModKit.ModModules
{
    internal class ForgeModConfig
    {
        // Mandatory fields
        public string ModLoader { get; set; } = "";
        public string LoaderVersion { get; set; } = "";
        public string License { get; set; } = "";

        // Optional fields
        public string IssueTrackerURL { get; set; } = "";
        public List<ForgeMod> Mods { get; set; } = new();

        // Dependency classes, but not used in your example
        public List<ForgeDependency> Dependencies { get; set; } = new();


    }

    internal class ForgeMod
    {
        // Mandatory fields
        public string ModId { get; set; } = "";
        public string Version { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";

        // Optional fields
        public string UpdateJSONURL { get; set; } = "";
        public string DisplayURL { get; set; } = "";
        public string LogoFile { get; set; } = "";
        public string Credits { get; set; } = "";
        public string Authors { get; set; } = "";

        public ModFile ToModFile()
        {
            ModFile file = new()
            {
                ID = ModId,
                Name = DisplayName,
                IconFile = LogoFile,
                Authors = new string[] { Authors },
                Description = Description,
                Version = Version,
            };
            return file;
        }
    }

    internal class ForgeDependency
    {
        public string ModId { get; set; } = "";
        public bool Mandatory { get; set; }
        public string VersionRange { get; set; } = "";
        public string Ordering { get; set; } = "";
        public string Side { get; set; } = "";
    }
}
