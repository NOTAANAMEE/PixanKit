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

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents a task for installing the NeoForge mod loader for Minecraft.
    /// </summary>
    public class NeoForgeInstaller : SequenceProgressTask
    {

        Folder _owner;

        string _name;

        string _version;

        JObject _neoforgeversion;

        string Installerpath { get => $"{Files.CacheDir}/Installer/neoforge.jar"; }

        string _url = "";

        FuncProgressTask<int> _initTask = new();

        AsyncProgressTask? _downloadTask;

        CliTask? _commandTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeoForgeInstaller"/> class.
        /// </summary>
        /// <param name="folder">The folder where NeoForge will be installed.</param>
        /// <param name="name">The name of the NeoForge installation.</param>
        /// <param name="mcversion">The Minecraft version for which NeoForge is being installed.</param>
        /// <param name="neoforgeversion">The JSON object containing the NeoForge version details.</param>
        public NeoForgeInstaller(Folder folder, string name, string mcversion, JObject neoforgeversion)
        {
            _owner = folder;
            _name = name;
            _version = mcversion;
            this._neoforgeversion = neoforgeversion;
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
            FileDownloadTask download = new("", Installerpath);
            _initTask.OnFinish += (a) =>
            {
                download.SetUrl(_url);
            };
            if (_owner.FindGame(_version) == null)
                _downloadTask.Add(new VanillaMinimalInstallTask(_owner, _version, _version));
            _downloadTask.Add(download);
            Add(_downloadTask);
        }

        private void AddCommandTask()
        {
            if (Launcher.Instance == null)
                throw new InvalidOperationException("Launcher hasn't inited yet");
            var java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes) ??
                throw new InvalidOperationException("No available java found");
            _commandTask = new(java.JavaExe, $"-jar \"{Installerpath}\" --installClient " +
                $"\"{_owner.FolderPath}\"");
            ProgressTasks.Add(_commandTask);
            _commandTask.OnFinish += (a) =>
            {
                GamePostProcess.Move(_owner, $"neoforge-{_neoforgeversion["version"]}", _name);
            };
        }

        private async Task<int> Init(Action<double> progress, CancellationToken token)
        {
            _url = await ServerList.ModLoaderServers["neoforge"]
                    .GetUrl(_neoforgeversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1.0);
            return 0;
        }
    }
}
