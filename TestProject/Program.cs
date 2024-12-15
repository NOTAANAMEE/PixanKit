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

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestFileDownload();
            //await DownloadFileMultithreadedAsync("https://piston-meta.mojang.com/mc/game/version_manifest.json", "a.json", 2);
        }

        

        static void TestFileDownload()
        {
            Files.Load();
            var tmp = new Launcher();
            //MultiThreadDownload task = new MultiThreadDownload("https://piston-meta.mojang.com/mc/game/version_manifest.json", "./a.json");
            //task.Start();
            //task.MainTask.Wait();
            tmp.Launch();
            
        }
    }
}
