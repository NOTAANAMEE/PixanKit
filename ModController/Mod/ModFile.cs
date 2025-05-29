using Newtonsoft.Json.Linq;
using PixanKit.ModController.Module;

namespace PixanKit.ModController.Mod;

/// <summary>
/// Represents a mod file. The class helps manage the version and dependencies
/// </summary>
/// <remarks>
/// Inits the mod file. It is not recommended to init it by yourself.
/// </remarks>
public class ModFile(string filepath)
{
    /// <summary>
    /// The metadata of the mod file. For example, the description and authors
    /// </summary>
    public ModMetaData? MetaData;

    /// <summary>
    /// Retrives the owner of the mod file.
    /// </summary>
    public ModCollection? Owner { get; internal set; }

    /// <summary>
    /// The version of the mod file.
    /// </summary>
    public required string Version;

    /// <summary>
    /// Retrieves the actual path of the file
    /// </summary>
    public string FilePath { get; internal set; } = filepath;

    /// <summary>
    /// Retrieves the file name of the file.
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// The list of the ID of the dependencies
    /// </summary>
    public required List<string> Dependencies = [];

    /// <summary>
    /// The release date of the mod file
    /// </summary>
    public required DateTime ReleaseDate;

    /// <summary>
    /// Whether the mod is a valid struture. 
    /// If true, load the id from the file.
    /// else, load the id from JSON data.
    /// </summary>
    public bool ValidStructure = true;

    /// <summary>
    /// Adds unique dependencies from the current object to the provided list.
    /// </summary>
    /// <param name="dependencies">
    /// A list of dependency strings to which unique dependencies will be added. 
    /// Existing dependencies in the list are not added again.
    /// </param>
    /// <remarks>
    /// This method iterates through the current object's dependencies and ensures 
    /// that only dependencies not already present in the input list are added. 
    /// It avoids duplicates in the input list.
    /// </remarks>
    public void GetDependencies(List<string> dependencies)
    {
        foreach (var dependency in Dependencies)
        {
            if (dependencies.Contains(dependency)) continue;
            dependencies.Add(dependency);
        }
    }

    /// <summary>
    /// The method enables the mod so that it will be activated while playing.
    /// </summary>
    public void Enable()
    {
        File.Move(FilePath, FilePath = FilePath[..^9]);
    }

    /// <summary>
    /// Disables the mod so that it will not be activated while playing
    /// </summary>
    public void Disable()
    {
        File.Move(FilePath, FilePath += ".disabled");
    }

    /// <summary>
    /// Save the ModFile as JSON data
    /// </summary>
    /// <returns>The JSON data</returns>
    public JObject ToJson()
    {
        return new()
        {
            {"id", MetaData?.ModId ??
                   throw new Exception("Mod not attatched to a ModMetaData") },
            {"release_date", ReleaseDate  },
            {"version", Version},
            {"depends", JArray.FromObject(Dependencies) }
        };
    }
}