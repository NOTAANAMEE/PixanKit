using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Folders;
using PixanKit.LaunchCore.JavaModule;
using PixanKit.LaunchCore.JavaModule.Java;
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
    readonly string _version = "";

    readonly Folder _owner;

    readonly string _name;

    static string Installerpath => $"{Files.CacheDir}/Installer/optifine.jar";

    //The Java Program I made myself. It is just used to handle the optifine install task
    static string Programpath => $"{Files.CacheDir}/Installer/optifineinstaller.jar";

    string _url = "";

    readonly JObject _optifineVersion;

    FuncProgressTask<int> _initProgressTask = new();

    AsyncProgressTask? _downloadTask;

    CliTask? _commandTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptifineInstaller"/> class.
    /// </summary>
    /// <param name="folder">The target folder where Minecraft is located.</param>
    /// <param name="name">The name of the Minecraft instance. The JAR file will be located at <c>folder\name\name.jar</c>.</param>
    /// <param name="mcversion">The Minecraft version for which Optifine is being installed.</param>
    /// <param name="optifineversion">A JSON object containing the Optifine version details.</param>
    public OptifineInstaller(Folder folder, string name, string mcversion, JObject optifineversion)
    {
        _name = name;
        _owner = folder;
        _version = mcversion;
        _optifineVersion = optifineversion;
        Init();
    }


    private void Init()
    {
        var file = Localize.PathLocalize($"{Files.CacheDir}/Installer/optifine.jar");
        _initProgressTask.Function += GetUrl;
        Add(_initProgressTask);
        AddDownloadTask();
        AddCommandTask();
    }

    private void AddDownloadTask()
    {
        _downloadTask = new();
        SimpleFileDownloadTask download = new("", Installerpath);
        _initProgressTask.OnFinish += (a) =>
        {
            download.SetUrl(_url);
        };
        if (_owner.FindGame(_version) == null)
        {
            _downloadTask.Add(new VanillaMinimalInstallTask(_owner, _version, _version));
        }
        _downloadTask.Add(download);
        Add(_downloadTask);
    }

    private void AddCommandTask()
    {
        JavaRuntime? java;
        if (Launcher.Instance == null) throw new InvalidOperationException("Init Launcher first");
        java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes);
        if (java == null) throw new Exception("No java found");

        var dir =
            $"{_version}-{(_optifineVersion["version"] ?? "").ToString().Replace(" ", "_")}";

        _commandTask = new(java.JavaExe, "-cp " +
                                         $"\"{Installerpath}{Localize.LocalParser}{Programpath}\" Program " +
                                         $"\"{_owner.FolderPath}\"");
        _commandTask.OnFinish += (a) =>
        {
            GamePostProcess.Move(_owner, dir, _name);
        };
        Add(_commandTask);
    }

    private async Task<int> GetUrl(Action<double> report, CancellationToken token)
    {
        _url = await ServerList.ModLoaderServers["optifine"]
            .GetUrl(_optifineVersion, token);
        return 0;
    }
}