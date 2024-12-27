using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.SystemInf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public virtual string GetLaunchArgument()
        {
            string arg = GetJVMArguments() + $" {className} " + GetGameArguments();
            arg = arg.Replace("${natives_directory}", $"\"{NativeDir}\"");
            arg = arg.Replace("${game_directory}", $"\"{RunningDir}\"");
            arg = arg.Replace("${assets_root}", $"\"{AssetsDir}\"");
            arg = arg.Replace("${assets_index_name}", GetAssetsID());
            arg = arg.Replace("${classpath}", "\"" + GetCPArgs() + "\"");
            arg = arg.Replace("${version_name}", Version);
            arg = arg.Replace("${version_type}", releaseType);
            arg = arg.Replace("${library_directory}", LibraryDir);
            arg = Localize.CPLocalize(arg);
            Logger.Info($"Arguments Generated. Targeted Game:{_path}");
            if (Settings == null) return arg;
            switch ((Settings["argument"] ?? 1).ToString())
            {
                case "overall":
                    arg = "${arguments} " + arg;
                    break;
                default:
                    arg = (Settings["arguments"] ?? 1).ToString() + " " + arg;
                    break;
            }
            return arg;
        }

        /// <summary>
        /// Checks whether the game is ready to launch.
        /// </summary>
        /// <returns><c>true</c> if the game can launch; otherwise, <c>false</c>.</returns>
        public virtual bool LaunchCheck() => true;

        /// <summary>
        /// Retrieves the Java Virtual Machine (JVM) arguments.
        /// </summary>
        /// <returns>A string containing the JVM arguments.</returns>
        protected virtual string GetJVMArguments()
            => javaArguments;

        /// <summary>
        /// Retrieves the game-specific arguments.
        /// </summary>
        /// <returns>A string containing the game arguments.</returns>
        protected virtual string GetGameArguments()
            => gameArguments;

        /// <summary>
        /// Retrieves game arguments for the same version game within the folder.
        /// </summary>
        /// <returns>A string containing the game arguments.</returns>
        /// <exception cref="Exception">Thrown if the same version game cannot be found.</exception>
        protected string SameVersionGameArguments()
        {
            var target = Owner?.FindVersion(_version, GameType.Original);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetGameArguments();
        }

        /// <summary>
        /// Retrieves the assets ID for the game.
        /// </summary>
        /// <returns>A string containing the assets ID.</returns>
        protected virtual string GetAssetsID()
            => assetsID;

        /// <summary>
        /// Retrieves the assets ID for the same version game within the folder.
        /// </summary>
        /// <returns>A string containing the assets ID.</returns>
        /// <exception cref="Exception">Thrown if the same version game cannot be found.</exception>
        protected string SameVersionAssetsID()
        {
            var target = Owner?.FindVersion(_version, GameType.Original);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetAssetsID();

        }

        /// <summary>
        /// Retrieves the classpath arguments for the game.
        /// </summary>
        /// <returns>
        /// A string containing the classpath arguments, excluding the game JAR file.
        /// </returns>
        /// <remarks>
        /// The classpath includes library paths and the main game JAR path.
        /// </remarks>
        protected virtual string GetCPArgs()
        {
            string classpath = "";
            foreach (LibraryBase library in libraries)
            {
                classpath += library.Path + Localize.LocalParser;
            }
            classpath += Path;
            return classpath;
        }

        /// <summary>
        /// Retrieves the classpath arguments for the same version game within the folder.
        /// </summary>
        /// <returns>A string containing the classpath arguments.</returns>
        /// <exception cref="Exception">Thrown if the same version game cannot be found.</exception>
        protected string SameVersionCPArgs()
        {
            var target = Owner?.FindVersion(_version, GameType.Original);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetCPArgs();
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
            foreach (LibraryBase library in libraries)
            {
                if (library.LibraryType != LibraryType.Native) continue;
                await Task.Run(
                    () => {
                        (library as NativeLibrary ?? new NativeLibrary()).Extract(LibraryDir, NativeDir);
                    }
                );
            }
        }

        private string GetRunningFolder()
        {
            if (Settings == null) return "";
            string runningfolder = (Settings["runningfolder"] ?? 1).ToString();
            string ret;
            switch (runningfolder)
            {
                case "overall":
                    if (Owner == null || Owner.Owner == null) throw new Exception("Can't use overall");
                    Settings["runningfolder"] = Owner.Owner.Settings["runningfolder"];
                    ret = GetRunningFolder();
                    Settings["runningfolder"] = JToken.FromObject("overall");
                    break;
                case "self":
                    ret = _path;
                    break;
                default:
                    ret = runningfolder;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// GRetrieves the optional args with different rules. Add the args after the command 
        /// as all of them are game arguments
        /// </summary>
        /// <param name="jData">Matching conditions</param>
        /// <returns>The arguments. Add them after the command</returns>
        public string GetOptionalArgs(JObject jData)
        {
            var ret = "";
            foreach (var arg in optionalArgs)
            {
                string argument = GetOptionalArgs(jData, arg);
                if (argument == "") continue;
                ret += $"{argument} ";
            }
            return ret;
        }

        private static string GetOptionalArgs(JObject jData, OptionalArgs arg)
        {
            foreach (var rule in arg.rules) 
            {
                if (!JudgeArg(jData, rule)) return "";
            }
            return arg.arg;
        }

        private static bool JudgeArg(JObject jData, string rule)
        {
            Regex reg = MyRegex();
            var matches = reg.Matches(rule);
            var match = matches.FirstOrDefault();

            string key = match?.Groups["Name"].Value ?? "";
            bool sign = match?.Groups["Sign"].Value == "=!";
            string condition = match?.Groups["Condition"].Value ?? "";

            if (!jData.ContainsKey(key)) return !sign;
            return sign == (jData[key]?.ToString() == condition);
        }

        [GeneratedRegex("(?<Name>\\w+)\\s*(?<Sign>=|=!)\\s*(?<Condition>[^\\s,]+)")]
        public static partial Regex MyRegex();
        #endregion
    }
}
