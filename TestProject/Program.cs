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
using Newtonsoft.Json.Linq;

namespace TestProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestFileDownload();
        }

        

        static async Task TestFileDownload()
        {
            Files.Load();
            _ = new Launcher();
            //MultiThreadDownload task = new MultiThreadDownload("https://piston-meta.mojang.com/mc/game/version_manifest.json", "./a.json");
            //task.Start();
            //task.MainTask.Wait();
            //tmp.Launch();
            //var jarr = ServerList.MinecraftVersionServer.GetVersions();
            //var jdata = ServerList.MinecraftVersionServer.GetLatestRelease(jarr);
            var jarr = await ServerList.ModLoaderServers["optifine"].GetVersionsForMinecraft("1.21.4");
            OptifineInstaller task = new(Launcher.Instance.Folders[0], "MyOptifine", "1.21.4", jarr.First as JObject);
            task.Start();
            //GetProgress(task);
            await task.MainTask;

        }

        private static int FindBuildsStart(List<string> array, string mcpatch, int lft, int rgh)
        {
            int current = (lft + rgh) / 2;
            int before = current - 1;
            string currentbuild = array[current];
            string beforebuild = array[before];
            if (currentbuild == mcpatch && beforebuild != currentbuild)
                return current;
            else if (lft >= rgh - 1) return -1;
            else if (beforebuild.CompareTo(mcpatch) >= 0)
                return FindBuildsStart(array, mcpatch, lft, current - 1);
            else return FindBuildsStart(array, mcpatch, current + 1, rgh);
        }

    }
}
