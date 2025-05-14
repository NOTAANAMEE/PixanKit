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
    /// Represents a task for installing the Quilt mod loader for Minecraft.
    /// </summary>
    public class QuiltInstaller : SequenceProgressTask
    {
        readonly Folder _owner;

        readonly string _name;

        readonly string _version;

        readonly JObject _quiltversion;

        readonly string _installerpath = $"{Files.CacheDir}/Installer/quilt.jar";

        string _url = "";

        FuncProgressTask<int> _initTask = new();

        AsyncProgressTask? _downloadTask;

        CliTask? _commandTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuiltInstaller"/> class.
        /// </summary>
        /// <param name="folder">The target folder where Minecraft is located.</param>
        /// <param name="name">The name of the Minecraft instance for which Quilt is being installed.</param>
        /// <param name="mcversion">The Minecraft version for which Quilt is being installed.</param>
        /// <param name="quiltversion">A JSON object containing the Quilt version details.</param>
        public QuiltInstaller(Folder folder, string name, string mcversion, JObject quiltversion)
        {
            _owner = folder;
            _name = name;
            _version = mcversion;
            this._quiltversion = quiltversion;
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
            FileDownloadTask download = new("", _installerpath);
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
            var java = JavaChooser.Newest(Launcher.Instance.JavaManager.JavaRuntimes) ??
                throw new Exception("No Java");
            var workingdir = Path.GetDirectoryName(_owner.FolderPath) ?? "./";
            _commandTask = new(java.JavaExe, $"-jar \"{Path.GetFullPath(_installerpath)}\" " +
                $"install client " +
                $"{_version} {_quiltversion["version"]} \"--install-dir=./.minecraft\"", workingdir);
            ProgressTasks.Add(_commandTask);
            _commandTask.OnFinish += (a) =>
            {
                GamePostProcess.Move(_owner,
                    $"quilt-loader-{_quiltversion["version"]}-{_version}", _name);
            };
        }

        private async Task<int> Init(Action<double> progress, CancellationToken token)
        {
            _url = await ServerList.ModLoaderServers["quilt"]
                    .GetUrl(_quiltversion, token);
            if (token.IsCancellationRequested) return 1;
            progress(1.0);
            return 0;
        }
    }
}
