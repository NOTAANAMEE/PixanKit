using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Exceptions;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.SystemInf;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.FuncTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Represents the task that only install the JSON file and the jar file of Minecraft
    /// </summary>
    public class VanillaMinimalInstallTask : SequenceProgressTask
    {
        readonly Folder Owner;

        readonly string name;

        readonly string version;

        readonly string path;

        readonly FuncProgressTask<int> InitTask = new();

        FileDownloadTask? jsondownload;

        FileDownloadTask? jardownload;

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
            Owner = folder;
            this.name = name;
            this.version = version;
            path = Path.Combine(folder.VersionDirPath, name);
            InitTask.Function += GetVersion;
            Init();
        }

        private void Init()
        {
            if (Directory.Exists(path)) throw new IOException($"Already Exists {path}");

            Directory.CreateDirectory(path);
            Add(InitTask);
            Add(jsondownload = new FileDownloadTask("", path + $"/{name}.json"));

            Add(jardownload = new FileDownloadTask("", path + $"/{name}.jar"));

            jsondownload.OnFinish += Task1Finish;
        }


        private async Task<int> GetVersion(Action<double> report, CancellationToken token)
        {
            JArray jArray;
            try
            {
                jArray = await ServerList.MinecraftVersionServer.GetVersionsAsync(token);
                foreach (var item in jArray)
                {
                    if (item["id"]?.ToString() == version)
                    {
                        jsondownload?.SetURL(item["url"]?.ToString() ??
                            throw new JSONKeyException(item, "url", "impossible"));
                        report?.Invoke(1);
                        return 0;
                    }
                }
                return 1;
            }
            catch { return 1; }
        }

        private void Task1Finish(ProgressTask task)
        {
            JObject mcjData = JObject.Parse(
                File.ReadAllText(Localize.PathLocalize($"{path}/{name}.json")));
            jardownload?.SetURL(mcjData["downloads"]?["client"]?["url"]?.ToString()??
                throw new JSONKeyException(mcjData, "downloads/client/url", "Version JSON document"));
        }
    }
}
