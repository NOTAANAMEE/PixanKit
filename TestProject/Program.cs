using PixanKit.LaunchCore;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.PlayerModule.Player;
using PixanKit;

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestFileDownload();
        }

        static void TestFileDownload()
        {
            Files.Load();
            Launcher launcher = new();
            //launcher.AddJavaRuntime("C:/Program Files/Java/jdk-21");
            //launcher.AddFolder(new Folder("D:/Program Files/Minecraft/.minecraft"));
            Logger.Info("TestProject.Program.Main", "Finished Folder");
            //launcher.AddPlayer(new OfflinePlayer("MyName"));
            var tmp = launcher.Launch();
            launcher.Close();
            Files.Save();

            Console.WriteLine(tmp.LogGZPath);
        }
    }
}
