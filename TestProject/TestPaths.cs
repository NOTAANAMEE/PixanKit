using PixanKit.LaunchCore.Extention;

namespace TestProject
{
    public static class TestPaths
    {
        public static void Test()
        {
            Paths.TrySet("test-folder", "D:/Minecraft");

            Console.WriteLine(Paths.GetOrAdd("test-folder2", "${test-folder}/CCC"));
        }
    }
}
