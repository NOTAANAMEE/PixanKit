using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Logger;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask;

/// <summary>
/// Represents a task for completing Minecraft game assets, including downloading the asset index
/// and the associated asset files.
/// </summary>
public class AssetsCompletionTask : SequenceProgressTask
{
    GameBase? _game;

    string _indexPath = "";

    private MultiFileDownloadTask? _task2;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetsCompletionTask"/> class.
    /// </summary>
    /// <param name="JObject">The JSON data containing the asset index information.</param>
    /// <param name="game">The game context to manage assets.</param>
    public AssetsCompletionTask(JObject JObject, GameBase game)
    {
        _game = game;
        Init(JObject);
    }


    internal AssetsCompletionTask() { }

    internal void Set(JObject jObject, GameBase game)
    {
        _game = game;
        Init(jObject);
    }

    private void Init(JObject jObject)
    {
        var url = jObject["assetIndex"]?["url"]?.ToString() ?? "";
        var index = jObject["assetIndex"]?["id"]?.ToString() ?? "";

        _indexPath = $"{_game?.AssetsDirPath}/indexes/{index}.json";

        if (File.Exists(Localize.PathLocalize(_indexPath)))
        {
            FileDownloadTask task;
            Add(task = new FileDownloadTask(url, _indexPath));
            task.OnFinish += _ => TaskFinish();
        }
        else TaskFinish();
        Add(_task2 = new MultiFileDownloadTask());
    }

    private void TaskFinish()
    {
        List<string> urls = [], paths = [];

        var jObject = JObject.Parse(File.ReadAllText(
            Localize.PathLocalize(_indexPath)
        ));
        foreach (var asset in jObject["objects"] ?? new JArray())
        {
            try
            {
                var hash = asset.First?["hash"]?.ToString() ?? "";
                var s = $"{hash[0..2]}/{hash}";
                var path = $"{_game?.AssetsDirPath}/objects/{s}";
                //Console.WriteLine(++count);
                if (File.Exists(Localize.PathLocalize(path))) continue;
                urls.Add(ServerList.MinecraftAssetsServer.GetAssetsUrl(hash));
                paths.Add(path);
            }
            catch (Exception ex)
            { Logger.Warn("PixanKit.ResourceDownloader", ex.Message); }

        }
        _task2?.Set([.. urls], [.. paths]);
    }
}