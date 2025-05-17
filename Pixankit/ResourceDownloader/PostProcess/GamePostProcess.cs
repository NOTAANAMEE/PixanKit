using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Json;

namespace PixanKit.ResourceDownloader.PostProcess;

/// <summary>
/// This class helps to process the game after downloading
/// </summary>
/// <param name="folder">the folder which contains the game</param>
/// <param name="name">the expected name of the game</param>
/// <param name="version">the version of the game</param>
/// <param name="processJson">whether process JSON document or not</param>
public class GamePostProcess(Folder folder, string name, string version, bool processJson)
{
    readonly string _name = name;

    readonly string _version = version;

    readonly string _versiondir = folder.VersionDirPath;

    readonly bool _processjson = processJson;

    readonly Folder _owner = folder;

    /// <summary>
    /// Start the process
    /// </summary>
    public void Process()
    {
        ProcessGame();
        if (_processjson) ProcessJson();
    }

    private void ProcessGame()
    {
        File.Copy($"{_versiondir}/{_version}/{_version}.jar",
            $"{_versiondir}/{_name}/{_name}.jar");
        if (_owner.FindGame(_version) != null) return;
        if (_owner.FindVersion(_version, GameType.Vanilla) != null)
        {
            Directory.Delete($"{_versiondir}/{_version}");
            return;
        }
        //var game = Initers.GameIniter($"{_versiondir}/{_name}");
        //if (!_processjson) 
        //  Launcher.Instance.GameManager.AddGame(game);
    }

    private void ProcessJson()
    {
        var target = Json.ReadFromFile($"{_versiondir}/{_version}/{_version}.json");
        var merge = Json.ReadFromFile($"{_versiondir}/{_name}/{_name}.json");

        Json.MergeJObject(target, merge);
        Json.SaveFile(_name, target);

        Directory.Delete($"{_versiondir}/{_version}");
    }

    /// <summary>
    /// Move the version folder to the new name
    /// </summary>
    /// <param name="folder">the foler which contains the game</param>
    /// <param name="loaderversion">the old name of the game</param>
    /// <param name="name">the name of the game</param>
    /// <returns>the new directory of the game</returns>
    public static string Move(Folder folder, string loaderversion, string name)
    {
        var folderpath = $"{folder.VersionDirPath}/{name}";
        var folderpathOld = $"{folder.VersionDirPath}/{loaderversion}";
        Directory.Move(folderpathOld, folderpath);
        foreach (var entry in Directory.GetFileSystemEntries(folderpath))
        {
            var filename = Path.GetFileName(entry);
            string newname;
            string newpath;
            if (!filename.StartsWith(loaderversion)) continue;
            newname = filename.Replace(loaderversion, name);
            newpath = $"{Path.GetDirectoryName(entry)}/{newname}";
            if (newname.EndsWith('/')) Directory.Move(entry, newpath);
            else File.Move(entry, newpath);
        }
        return folderpath;
    }
}