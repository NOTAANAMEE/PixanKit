using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Exceptions;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.Json;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask;

/// <summary>
/// Represents the task that only install the JSON file and the jar file of Minecraft
/// </summary>
public class VanillaMinimalInstallTask : SequenceProgressTask
{

    readonly string _name;

    readonly string _version;

    readonly string _path;

    readonly FuncProgressTask<int> _initTask = new();

    FileDownloadTask? _jsonTask;

    FileDownloadTask? _jarTask;

    /// <summary>
    /// Initor
    /// </summary>
    /// <param name="folder">The Owner Of The Game</param>
    /// <param name="name">The Name Of The Game</param>
    /// <param name="version">The Version Of Minecraft</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public VanillaMinimalInstallTask(Folder folder, string name, string version)
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

        Add(_jarTask = new FileDownloadTask("", _path + $"/{_name}.jar"));

        _jsonTask.OnFinish += Task1Finish;
    }


    private async Task<int> GetVersion(Action<double> report, CancellationToken token)
    {
        try
        {
            var jArray = await ServerList.MinecraftVersionServer.GetVersionsAsync(token);
            foreach (var item in jArray)
            {
                if (item["id"]?.ToString() != _version) continue;
                _jsonTask?.SetUrl(item["url"]?.ToString() ??
                                  throw new JsonKeyException(item, "url", "impossible"));
                report.Invoke(1);
                return 0;
            }
            return 1;
        }
        catch { return 1; }
    }

    private void Task1Finish(ProgressTask task)
    {
        var mcjData = JObject.Parse(
            File.ReadAllText(Localize.PathLocalize($"{_path}/{_name}.json")));
        _jarTask?.SetUrl(mcjData.GetValue(Format.ToString, "downloads/client/url") ??
                             throw new JsonKeyException(mcjData, "downloads/client/url", "Version JSON document"));
    }
}