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
/// Represents a task for installing Optifine, a Minecraft optimization mod.
/// </summary>
public class OptifineInstaller : SequenceProgressTask
{
    private readonly string _version;

    private readonly Folder _owner;

    private readonly string _name;

    private static string InstallerPath => $"{Files.CacheDir}Installer/optifine.jar";

    //The Java Program I made myself. It is just used to handle the optifine install task
    private static string HelperPath => $"{Files.CacheDir}Installer/optifineinstaller.jar";

    private string _url = "";

    private readonly JObject _optifineVersion;

    private readonly FuncProgressTask<int> _initProgressTask = new();

    private AsyncProgressTask? _downloadTask;

    private CliTask? _commandTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptifineInstaller"/> class.
    /// </summary>
    /// <param name="folder">The target folder where Minecraft is located.</param>
    /// <param name="name">The name of the Minecraft instance. The JAR file will be located at <c>folder\name\name.jar</c>.</param>
    /// <param name="gameVersion">The Minecraft version for which Optifine is being installed.</param>
    /// <param name="optifineVerObj">A JSON object containing the Optifine version details.</param>
    public OptifineInstaller(Folder folder, string name, string gameVersion, JObject optifineVerObj)
    {
        _name = name;
        _owner = folder;
        _version = gameVersion;
        _optifineVersion = optifineVerObj;
        Init();
    }


    private void Init()
    {
        _initProgressTask.Function += GetUrl;
        Add(_initProgressTask);
        AddDownloadTask();
        AddCommandTask();
    }

    private void AddDownloadTask()
    {
        _downloadTask = new();
        SimpleFileDownloadTask download = new("", InstallerPath);
        _initProgressTask.OnFinish += _ => download.SetUrl(_url);
        if (_owner.FindGame(_version) == null)
        {
            _downloadTask.Add(new VanillaMinimalInstallTask(_owner, _version, _version));
        }
        _downloadTask.Add(download);
        Add(_downloadTask);
    }

    private void AddCommandTask()
    {
        if (Launcher.Instance == null) throw new InvalidOperationException("Init Launcher first");
        var java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes);
        if (java == null) throw new Exception("No java found");

        var dir =
            $"{_version}-{(_optifineVersion["version"] ?? "").ToString().Replace(" ", "_")}";

        _commandTask = new(java.JavaExe, "-cp " +
                                         $"\"{InstallerPath}{Localize.LocalParser}{HelperPath}\" Program " +
                                         $"\"{_owner.FolderPath}\"");
        _commandTask.OnFinish += _ => 
            GamePostProcess.Move(_owner, dir, _name);
        Add(_commandTask);
    }

    private async Task<int> GetUrl(Action<double> report, CancellationToken token)
    {
        _url = await ServerList.ModLoaderServers["optifine"]
            .GetUrl(_optifineVersion, token);
        return 0;
    }
}