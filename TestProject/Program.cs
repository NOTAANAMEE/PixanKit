using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.ModController;
using PixanKit.ModController.Module;

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Files.Load();
            ModModule.DefaultFile();
            ModModule.Init();
            Launcher launcher = new();
            ModModule _ = new();
            //await Task.WhenAll(ModModule.Instance.InitTasks);
        }

    }
}
