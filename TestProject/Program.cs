using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.PlayerModule.Player;
using PixanKit.ModController;
using PixanKit.ModController.Module;

namespace TestProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ModModule.Init();
            Files.Load();
            _ = new Launcher();
            ModModule modModule = new();
            modModule.AddCollection(Launcher.Instance.Folders.First().FindGame("Create Train") as ModdedGame);
            await Task.WhenAll(modModule.InitTasks);
            Console.WriteLine(modModule.ModdedGames.Values.First().GetDependencies());
        }

        

        static async Task TestFileDownload()
        {
            

        }

    }
}
