using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.PostProcess;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask;

/// <summary>
/// Represents a task for installing the Quilt mod loader for Minecraft.
/// </summary>
public class QuiltInstaller : SequenceProgressTask
{
    private readonly Folder _owner;

    private readonly string _name;

    private readonly string _version;

    private readonly JObject _quiltVerObj;

    private static string InstallerPath => $"{Files.CacheDir}Installer/quilt.jar";

    private string _url = "";

    private readonly FuncProgressTask<int> _initTask = new();

    private AsyncProgressTask? _downloadTask;

    private CliTask? _commandTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuiltInstaller"/> class.
    /// </summary>
    /// <param name="folder">The target folder where Minecraft is located.</param>
    /// <param name="name">The name of the Minecraft instance for which Quilt is being installed.</param>
    /// <param name="gameVersion">The Minecraft version for which Quilt is being installed.</param>
    /// <param name="quiltVerObj">A JSON object containing the Quilt version details.</param>
    public QuiltInstaller(Folder folder, string name, string gameVersion, JObject quiltVerObj)
    {
        _owner = folder;
        _name = name;
        _version = gameVersion;
        this._quiltVerObj = quiltVerObj;
        Init();
    }

    private void Init()
    {
        _initTask.Function += Init;
        Add(_initTask);
        AddDownloadTask();
        AddCommandTask();
    }

    private void AddDownloadTask()
    {
        _downloadTask = new();
        FileDownloadTask download = new("", InstallerPath);
        _initTask.OnFinish += _ => download.SetUrl(_url);
        if (_owner.FindGame(_version) == null)
            _downloadTask.Add(new VanillaMinimalInstallTask(_owner, _version, _version));
        _downloadTask.Add(download);
        Add(_downloadTask);
    }

    private void AddCommandTask()
    {
        var java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes) ??
                   throw new Exception("No Java");
        var workingDir = Path.GetDirectoryName(_owner.FolderPath) ?? "./";
        _commandTask = new(java.JavaExe, $"-jar \"{Path.GetFullPath(InstallerPath)}\" " +
                                         $"install client " +
                                         $"{_version} {_quiltVerObj["version"]} \"--install-dir=./.minecraft\"", workingDir);
        ProgressTasks.Add(_commandTask);
        _commandTask.OnFinish += _ =>
            GamePostProcess.Move(_owner,
                $"quilt-loader-{_quiltVerObj["version"]}-{_version}", _name);
    }

    private async Task<int> Init(Action<double> progress, CancellationToken token)
    {
        _url = await ServerList.ModLoaderServers["quilt"]
            .GetUrl(_quiltVerObj, token);
        if (token.IsCancellationRequested) return 1;
        progress(1.0);
        return 0;
    }
}