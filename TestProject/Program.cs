using PixanKit.LaunchCore;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.PlayerModule.Player;
using PixanKit;
using PixanKit.ModModule.Module;
using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using PixanKit.LaunchCore.Server;
using PixanKit.ResourceDownloader.Download.InstallTask;
using PixanKit.ResourceDownloader.Tasks;

namespace TestProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestFileDownload();
            //await DownloadFileMultithreadedAsync("https://piston-meta.mojang.com/mc/game/version_manifest.json", "a.json", 2);
        }

        

        static async Task TestFileDownload()
        {
            Files.Load();
            var tmp = new Launcher();
            //MultiThreadDownload task = new MultiThreadDownload("https://piston-meta.mojang.com/mc/game/version_manifest.json", "./a.json");
            //task.Start();
            //task.MainTask.Wait();
            //tmp.Launch();
            //var jarr = ServerList.MinecraftVersionServer.GetVersions();
            //var jdata = ServerList.MinecraftVersionServer.GetLatestRelease(jarr);
            OriginalInstallTask task = new(Launcher.Instance.Folders[0], "MyName", "1.21.4");
            task.Start();
            GetProgress(task);
            await task.MainTask;

        }

        static async void GetProgress(OriginalInstallTask task)
        {
            while (task.Progress != 1)
            {
                Console.WriteLine(task.Progress);
                Console.WriteLine($"act:{task.act.Status}, lct:{task.lct.Status}, fdt:{task.dt.Status}");
                await Task.Delay(1000);
            }
        }
    }
}
