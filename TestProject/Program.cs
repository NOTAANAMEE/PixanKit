using PixanKit.LaunchCore;
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Param;
using PixanKit.LaunchCore.PlayerModule.Player;

namespace TestProject
{
    internal class Program
    {
        static void Main()
        {
            Files.Load();
            Launcher launcher = Launcher.Instance;
            launcher.Init(); // Initialize the launcher
            Console.WriteLine("PixanKit Launcher started.");
            var tmp = launcher.Launch();

            tmp.SaveToFile();
            launcher.Close();
            Files.Save(); // Ensure to save the state after closing the launcher
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
