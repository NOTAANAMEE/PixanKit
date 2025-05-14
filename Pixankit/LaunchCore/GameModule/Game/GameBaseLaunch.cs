using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.SystemInf;

namespace PixanKit.LaunchCore.GameModule.Game
{
    public abstract partial class GameBase
    {
        #region LaunchMethods
        /// <summary>
        /// Generates the launch arguments for the game.
        /// </summary>
        /// <returns>A formatted string containing the full launch arguments.</returns>
        /// <remarks>
        /// This method replaces placeholders like <c>${natives_directory}</c> with actual paths
        /// and applies localization. It also incorporates JVM and game-specific arguments.
        /// </remarks>
        public string GetLaunchArgument()
        {
            string arg = GetArgument();
            arg = arg.Replace("${natives_directory}", $"\"{NativeDirPath}\"");
            arg = arg.Replace("${game_directory}", $"\"{GameRunningDirPath}\"");
            arg = arg.Replace("${assets_root}", $"\"{AssetsDirPath}\"");
            arg = arg.Replace("${assets_index_name}", GetAssetsId());
            arg = arg.Replace("${classpath}", "\"" + GetCpArgs() + "\"");
            arg = arg.Replace("${version_name}", Version);
            arg = arg.Replace("${version_type}", ReleaseType);
            arg = arg.Replace("${library_directory}", LibrariesDirPath);
            arg = Localize.CpLocalize(arg);
            Logger.Logger.Info($"Arguments Generated. Targeted Game:{GameFolderPath}");
            arg = (Settings["argument"] ?? 1).ToString() switch
            {
                "overall" => "${arguments} " + arg,
                _ => (Settings["arguments"] ?? "") + " " + arg,
            };
            return arg;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetArgument()
            => Params.JavaArgs + $" {Params.MainClass} " + Params.GameArgs;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetAssetsId()
            => Params.AssetsId;

        /// <summary>
        /// Checks whether the game is ready to launch.
        /// </summary>
        /// <returns><c>true</c> if the game can launch; otherwise, <c>false</c>.</returns>
        public virtual bool LaunchCheck() => true;

        /// <summary>
        /// Retrieves the classpath arguments for the game.
        /// </summary>
        /// <returns>
        /// A string containing the classpath arguments, excluding the game JAR file.
        /// </returns>
        /// <remarks>
        /// The classpath includes library paths and the main game JAR path.
        /// </remarks>
        protected virtual string GetCpArgs()
        {
            string classpath = LibrariesRef.Libraries.Aggregate("", (current, library) => current + (library.LibraryPath + Localize.LocalParser));
            classpath += GameJarFilePath;
            return classpath;
        }

        internal static string GetCpArgs(IEnumerable<LibraryBase> libs)
        {
            return libs.Aggregate("", (current, library) => current + (library.LibraryPath + Localize.LocalParser));
        }
        

        /// <summary>
        /// Decompresses native libraries required for the game.
        /// </summary>
        /// <remarks>
        /// This method extracts all native libraries to the <c>NativeDir</c> folder.
        /// It skips non-native libraries.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task Decompress()
        {
            await LibrariesRef.Extract(this.LibrariesDirPath, this.NativeDirPath);
        }

        private string GetRunningFolder()
        {
            string s = (Settings["runningfolder"] ?? "self").ToString();
            if (s == "overall") s = 
                Launcher.Instance.Settings["runningfolder"]?.ToString() 
                                    ?? "self";
            return s switch
            {
                "self" => GameFolderPath,
                _ => s
            };
        }
        #endregion
    }
}
