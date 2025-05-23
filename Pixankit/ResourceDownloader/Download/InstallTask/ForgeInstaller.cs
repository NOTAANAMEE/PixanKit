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
/// Represents a task for installing the Forge mod loader for Minecraft.
/// </summary>
public class ForgeInstaller : SequenceProgressTask
{
    private readonly Folder _owner;

    private readonly string _name;

    private readonly string _version;

    private readonly JObject _forgeVerObj;

    private static string InstallerPath => $"{Files.CacheDir}Installer/forge.jar";

    private string _url = "";

    private readonly FuncProgressTask<int> _initTask = new();

    private AsyncProgressTask? _downloadTask;

    private CliTask? _commandTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgeInstaller"/> class.
    /// </summary>
    /// <param name="folder">The folder where Forge will be installed.</param>
    /// <param name="name">The name of the Forge installation.</param>
    /// <param name="gameVersion">The Minecraft version for which Forge is being installed.</param>
    /// <param name="forgeVerObj">The JSON object containing the Forge version details.</param>
    public ForgeInstaller(Folder folder, string name, string gameVersion, JObject forgeVerObj)
    {
        _owner = folder;
        _name = name;
        _version = gameVersion;
        _forgeVerObj = forgeVerObj;
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
        if (Launcher.Instance == null) throw new Exception();
        var java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes) ??
                   throw new Exception();
        _commandTask = new(java.JavaExe, $"-jar \"{InstallerPath}\" --installClient " +
                                         $"\"{_owner.FolderPath}\"");
        ProgressTasks.Add(_commandTask);
        _commandTask.OnFinish += _ =>
            GamePostProcess.Move(_owner, $"{_version}-forge-{_forgeVerObj["version"]}", _name);
    }

    private async Task<int> Init(Action<double> progress, CancellationToken token)
    {
        _url = await ServerList.ModLoaderServers["forge"]
            .GetUrl(_forgeVerObj, token);
        if (token.IsCancellationRequested) return 1;
        progress(1.0);
        return 0;
    }
}