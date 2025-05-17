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
/// Represents a task for installing the Fabric mod loader for Minecraft.
/// </summary>
public class FabricInstaller : SequenceProgressTask
{
    Folder _owner;

    string _name;

    string _version;

    JObject _fabricversion;

    string Installerpath => $"{Files.CacheDir}/Installer/fabric.jar";

    string _fabricversioname = "";

    string _url = "";

    FuncProgressTask<int> _initTask = new();

    AsyncProgressTask? _downloadTask;

    CliTask? _commandTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="FabricInstaller"/> class.
    /// </summary>
    /// <param name="folder">The target folder where Minecraft is located.</param>
    /// <param name="name">The name of the Minecraft instance for which Fabric is being installed.</param>
    /// <param name="mcversion">The Minecraft version for which Fabric is being installed.</param>
    /// <param name="fabricversion">A JSON object containing the Fabric version details.</param>
    public FabricInstaller(Folder folder, string name, string mcversion, JObject fabricversion)
    {
        _owner = folder;
        _name = name;
        _version = mcversion;
        this._fabricversion = fabricversion;
        Init();
    }

    private void Init()
    {
        _initTask.Function += Init;
        Add(_initTask);
        AddDownloadTask();
        AddCommandTask();
        _fabricversioname = $"fabric-loader-{_fabricversion["version"]}-{_version}";
        this.OnFinish += (a) => { FinishTask(); };
    }

    private void AddDownloadTask()
    {
        _downloadTask = new();
        FileDownloadTask download = new("", Installerpath);
        _initTask.OnFinish += (a) =>
        {
            download.SetUrl(_url);
        };
        _downloadTask.Add(download);
        if (_owner.FindGame(_version) == null)
            _downloadTask.Add(new VanillaMinimalInstallTask(_owner, _version, _version));
        Add(_downloadTask);
        //DownloadTask.OnFinish += (a) => { Console.WriteLine("DownloadTask Finish"); };
    }

    private void AddCommandTask()
    {
        if (Launcher.Instance == null)
            throw new InvalidOperationException("Launcher hasn't inited yet");
        var java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes) ??
                   throw new InvalidOperationException("No java found");
        _commandTask = new(java.JavaExe, $"-jar \"{Installerpath}\" client " +
                                         $"-dir \"{_owner.FolderPath}\" -mcversion {_version} -loader {_fabricversion["version"]} " +
                                         $"\"{_owner.FolderPath}\"");
        Add(_commandTask);
    }

    private void FinishTask()
    {
        GamePostProcess.Move(_owner, _fabricversioname, _name);
    }

    private async Task<int> Init(Action<double> progress, CancellationToken token)
    {
        _url = await ServerList.ModLoaderServers["fabric"]
            .GetUrl(_fabricversion, token);
        if (token.IsCancellationRequested) return 1;
        progress(1);
        return 0;
    }
}