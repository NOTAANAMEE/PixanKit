using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Files.Load();
            Launcher launcher = new();
        }

    }
}
