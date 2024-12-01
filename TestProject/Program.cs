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

        

        static async void TestFileDownload()
        {
            //Files.Load();
            //_ = new Launcher();
            //MultiThreadDownload task = new MultiThreadDownload("https://piston-meta.mojang.com/mc/game/version_manifest.json", "./a.json");
            //task.Start();
            //task.MainTask.Wait();
            FileDownloadTask task = new("https://piston-data.mojang.com/v1/objects/aee14fb6019ac3460574eaecdad3b66a96dfd6e6/client.jar", "./a.jar");
            task.OnException += (Exception ex) => 
            { 
                Console.WriteLine(ex.ToString()); 
            };
            task.OnReport += (double progress) => Console.WriteLine(progress);
            task.Start();
            await task.MainTask;
        }
    }
}
