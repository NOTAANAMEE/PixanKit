using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask;

/// <summary>
/// 
/// </summary>
public class VanillaInstallTask : SequenceProgressTask
{
    /// <summary>
    /// The Return Game
    /// </summary>
    public GameBase? Game => _game;

    GameBase? _game;

    readonly string _name;

    readonly string _version;

    readonly string _path;

    private readonly FuncProgressTask<int> _initTask = new();

    FileDownloadTask? _jsonTask;

    readonly AsyncProgressTask _asyncTask = new();

    LibraryCompletionTask? _libraryTask;

    FileDownloadTask? _jarTask;

    AssetsCompletionTask? _assetsTask;


    /// <summary>
    /// Initor
    /// </summary>
    /// <param name="folder">The Owner Of The Game</param>
    /// <param name="name">The Name Of The Game</param>
    /// <param name="version">The Version Of Minecraft</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public VanillaInstallTask(Folder folder, string name, string version)
    {
        this._name = name;
        this._version = version;
        _path = Path.Combine(folder.VersionDirPath, name);
        _initTask.Function += GetVersion;
        Init();
    }

    private void Init()
    {
        if (Directory.Exists(_path)) throw new IOException($"Already Exists {_path}");

        Directory.CreateDirectory(_path);
        Add(_initTask);
        Add(_jsonTask = new FileDownloadTask("", _path + $"/{_name}.json"));

        _asyncTask.Add(_jarTask = new FileDownloadTask("", _path + $"/{_name}.jar"));
        _asyncTask.Add(_libraryTask = new LibraryCompletionTask());
        _asyncTask.Add(_assetsTask = new AssetsCompletionTask());
        Add(_asyncTask);

        _jsonTask.OnFinish += Task1Finish;
    }

    private void Task1Finish(ProgressTask task)
    {
        //Console.WriteLine("Task 0 Finished");

        var mcjData = JObject.Parse(
            File.ReadAllText(Localize.PathLocalize($"{_path}/{_name}.json")));
        _game = Launcher.Instance.GameManager.AddGameAndReturn(_path);
        _jarTask?.SetUrl(mcjData["downloads"]?["client"]?["url"]?.ToString() ?? "");
        _libraryTask?.Set(_game);
        _assetsTask?.Set(mcjData, _game);
    }

    private async Task<int> GetVersion(Action<double> report, CancellationToken token)
    {
        var jArray = await ServerList.MinecraftVersionServer.GetVersionsAsync(token);
        if (token.IsCancellationRequested) return 1;
        foreach (var item in jArray)
        {
            if (item["id"]?.ToString() != _version) continue;
            _jsonTask?.SetUrl(item["url"]?.ToString() ?? throw new Exception());
            report.Invoke(1);
            return 0;
        }
        throw new Exception();
    }
}