using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.LibraryData;
using PixanKit.LaunchCore.Log;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// The Base Of The Game
    /// </summary>
    public abstract class GameBase
    {
        #region Properties
        /// <summary>
        /// Minecraft Game Name
        /// </summary>
        public string Name { get => System.IO.Path.GetFileName(_path) ?? ""; }

        /// <summary>
        /// Minecraft Game Description
        /// </summary>
        public string Description 
        { 
            get => (Settings["description"]?? "Minecraft").ToString();
            set => Settings["description"] = value;
        }

        /// <summary>
        /// Minecraft Game Folder. Where game.jar is at.
        /// </summary>
        public string GameFolder { get =>_path; }

        /// <summary>
        /// The Owner Folder
        /// </summary>
        public Folder? Owner { get => folder; }

        /// <summary>
        /// The actual path of jar file
        /// </summary>
        public string Path { get => _path + $"/{Name}.jar"; }

        /// <summary>
        /// The path of JSON file
        /// </summary>
        public string JsonPath { get => folder + $"/{Name}.json"; }

        /// <summary>
        /// The place to store world data and mods
        /// gamedir
        /// </summary>
        public string RunningDir
        {
            get => GetRunningFolder();
            set => Settings["runningfolder"] = value;
        }

        /// <summary>
        /// The place to store library folder
        /// </summary>
        public string LibraryDir
        {
            get => (folder == null)? RootDir + "/libraries":folder.LibraryDir;
        }

        /// <summary>
        /// The place to store Textrue Text and Assets
        /// </summary>
        public string AssetsDir 
        { 
            get => (folder == null)? _path.Remove(_path.LastIndexOf("/versions/")) + "/assets"
                :folder.AssetsDir; 
        }

        /// <summary>
        /// The root folder
        /// </summary>
        public string RootDir{ get => _path.Remove(_path.LastIndexOf("/versions/")); }

        /// <summary>
        /// The place to get Minecraft Version
        /// </summary>
        public string Version { get => _version; }        

        /// <summary>
        /// The place to store native binary libraries
        /// </summary>
        public string NativeDir { get => _path + $"/{Name}-natives"; }

        /// <summary>
        /// The Settings Config Path
        /// </summary>
        public string SettingsPath { get => _path + Files.GameSettingName; }

        /// <summary>
        /// The Actual path of crash report. tar.gz file
        /// </summary>
        public string CrashReportDir { get => RunningDir + "/crash-reports"; }

        /// <summary>
        /// Get The Type Of Game
        /// </summary>
        public GameType GameType { get => _gameType; }

        /// <summary>
        /// Settings
        /// </summary>
        public JObject Settings = new()
        {
            { "java", "overall"},//"overall": the same as the overall settings, "specified": Should be the same version, "closest": The closest version(Bigger / equal), "newest": The largest version, default: user specified
            { "argument", "overall" },//"overall": the same as the overall settings, default:user specified
            { "runningfolder", "self" }, //"overall":the same as the overall settings, "self": self folder defult: user specified
            { "description", "A Minecraft Game" }
        };
        #endregion

        #region Fields
        internal JObject tmpdata = new JObject();

        internal Folder? folder = null;

        internal string _path = "";

        internal string _version = "";

        internal GameType _gameType;

        internal List<LibraryBase> libraries = new();

        internal string className = "";

        internal string gameArguments = "";

        internal string javaArguments = "";

        internal short javaVersion = 8;

        internal string assetsID = "";

        internal string releaseType = "";
        #endregion

        #region Initors
        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="path">The version path. Like "C:/Users/Admin/AppData/Roaming/.minecraft/versions/1.20.1"</param>
        /// <param name="jData">The JSON file.</param>
        public GameBase(string path, JObject jData):this(path, false)
        {
            tmpdata = jData;
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="path">The Path like:<c>"C:\\Users\\Admin\\AppData\\Roaming\\.minecraft\\versions\\1.14"</c></param>
        /// <param name="initFromFile">If True, Read The File And Init</param>
        protected GameBase(string path, bool initFromFile) 
        {
            _path = path;
            if (initFromFile)
            {
                tmpdata = ReadJObj(path);
            }
            Logger.Info($"Game Base Added. Path:{_path}");
        }

        /// <summary>
        /// Initor
        /// </summary>
        /// <param name="jData">JSON Data Of The Game</param>
        protected GameBase(JObject jData):this("", jData) { }
        #endregion

        #region InitorUsingMethods
        /// <summary>
        /// Init JSON Data From Outside
        /// </summary>
        /// <param name="jData"></param>
        public void InitJData(JObject jData)
        {
            tmpdata = jData;
            InitJData();
        }

        private void InitJData()
        { 
            var jData = tmpdata;
            className = (jData["mainClass"] ?? "net.minecraft.client.main.Main.").ToString();
            SetLibrary(jData);
            SetGameArgument(jData);
            SetJVMArguments(jData);

            //Set The Version. Real Version
            if (jData["inheritsfrom"] != null) _version = (jData["inheritsfrom"] ?? "").ToString();
            _version = (jData["id"] ?? "").ToString();
            releaseType = (jData["type"] ?? "release").ToString();

            SetSettings();
            tmpdata = new();
        }

        /// <summary>
        /// Set the folder of the game
        /// </summary>
        /// <param name="owner">folder. Should be the actual folder that it is in</param>
        public virtual void SetOwner(Folder owner)
        {
            if (_path.StartsWith(owner.Path))
            folder = owner;
            InitJData();
            
            //Launcher.GameInit?.Invoke(Owner.Owner, this);
        }

        private void SetSettings()
        {
            string path = Localize.PathLocalize(SettingsPath);
            if (File.Exists(path))
                Settings = JObject.Parse(
                    File.ReadAllText(path));
        }

        private void SetLibrary(JObject jData)
        {
            foreach (JToken token in (jData["libraries"] ?? new JObject()))
            {
                LibraryBase? tmp = null;

                if (!LibraryBase.GetAllowedSystem(token).Contains(SystemInformation.OSName)) continue;//不支持就下一个

                if (folder != null && (tmp = folder.FindLibrary((token["name"] ?? 1).ToString())) != null)
                {
                    libraries.Add(tmp);
                    continue;
                }

                switch (LibraryBase.GetLibraryType(token))
                {
                    case LibraryType.Ordinary:
                        libraries.Add(folder.AddLibrary(new OrdinaryLibrary(token)));
                        break;
                    case LibraryType.Native:
                        libraries.Add(folder.AddLibrary(new NativeLibrary(token)));
                        if ((token["downloads"] as JObject ?? new JObject()).Count > 1) 
                            libraries.Add(folder.AddLibrary(new OrdinaryLibrary(token)));
                        break;
                    case LibraryType.Mod:
                        libraries.Add(folder.AddLibrary(tmp = new LoaderLibrary(token)));
                        break;
                }
            }
            Logger.Info($"Libraries Added. Number:{libraries.Count}");
        }

        internal virtual void SetGameArgument(JObject jData)
        {
            if (jData["minecraftArguments"] != null) 
            {
                gameArguments = (jData["minecraftArguments"] ?? 1).ToString();
            }
            else
            {
                foreach(JToken token in (jData["arguments"] ?? new JObject())["game"]?? new JObject())
                {
                    if (token.Type != JTokenType.String) continue;
                    gameArguments += token.ToString() + " ";
                }
            }
        }

        internal virtual void SetJVMArguments(JObject jData)
        {
            string tmp = "[{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"osx\"}}],\"value\": [\"-XstartOnFirstThread\"]},{\"rules\": [{\"action\": \"allow\",\"os\": {\"name\": \"windows\"}}],\"value\": \"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump\"},{\"rules\": [{\"action\": \"allow\",\"os\": {\"arch\": \"x86\"}}],\"value\": \"-Xss1M\"},\"-Djava.library.path=${natives_directory}\"" +
                ",\"-Dminecraft.launcher.brand=${launcher_name}\",\"-Dminecraft.launcher.version=${launcher_version}\",\"-cp\",\"${classpath}\"]";
            JArray? jvmargArray;
            if (jData["arguments"] != null && (jData["arguments"] ?? new JObject())["jvm"] != null)
            {
                jvmargArray = (jData["arguments"] ?? new JObject())["jvm"] as JArray;
            }
            else
            {
                jvmargArray = JArray.Parse(tmp);
            }
            if (jvmargArray == null) return;
            foreach(JToken token in jvmargArray)
            {
                if (token.Type == JTokenType.String) 
                {
                    string tok = token.ToString();
                    if (tok.Contains(' ')) tok = "\"" + tok + "\"";
                    javaArguments += tok + " "; 
                    continue; 
                }
                if (LibraryBase.GetAllowedSystem(token).Contains(SystemInformation.OSName))
                {
                    string tmpstr;
                    if ((token["value"]?? 0).Type == JTokenType.String) tmpstr = (token["value"] ?? 1).ToString();
                    else tmpstr = string.Join(' ', token["value"] as JArray?? new JArray());
                    javaArguments += tmpstr + " ";
                }
            }
        }

        private static JObject ReadJObj(string path)
        {
            JObject jData;
            {
                string tmpcontent = File.ReadAllText(path);
                jData = JObject.Parse(tmpcontent);
            }
            return jData;
        }
        #endregion

        #region LaunchMethods
        /// <summary>
        /// This will generate the launch argument of Minecraft.
        /// </summary>
        /// <returns>The argument. You still need to specify Java, and player arguments
        /// </returns>
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
        /// Check whether it could launch
        /// </summary>
        /// <returns>if could return true</returns>
        public virtual bool LaunchCheck() => true;

        /// <summary>
        /// Get The Java VM Arguments
        /// </summary>
        /// <returns>The Java VM Arguments String</returns>
        protected virtual string GetJVMArguments()
            => javaArguments;

        /// <summary>
        /// Get The Game Arguments
        /// </summary>
        /// <returns>Game Arguments String</returns>
        protected virtual string GetGameArguments()
            => gameArguments;

        /// <summary>
        /// Run <c>GetGameArguments()</c> For The Same Version Game Inside The Folder
        /// </summary>
        /// <returns>Game Arguments String</returns>
        /// <exception cref="Exception"></exception>
        protected string SameVersionGameArguments()
        {
            var target = Owner.FindVersion(_version, GameType.Ordinary);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetGameArguments();
        }

        /// <summary>
        /// Get The Assets ID
        /// </summary>
        /// <returns>The Assets ID String</returns>
        protected virtual string GetAssetsID()
            => assetsID;

        /// <summary>
        /// Run <c>GetAssetsID()</c> For The Same Version Game Inside The Folder
        /// </summary>
        /// <returns>The Assets ID String</returns>
        /// <exception cref="Exception"></exception>
        protected string SameVersionAssetsID()
        {
            var target = Owner.FindVersion(_version, GameType.Ordinary);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.GetAssetsID();

        }

        /// <summary>
        /// Get The Class Path Args
        /// </summary>
        /// <returns>The Class Path String Which Does Not Contains The Jar File Of The Game</returns>
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
        /// Run <c>GetCPArgs()</c> For The Same Version Game Inside The Folder
        /// </summary>
        /// <returns>Class Path Args</returns>
        /// <exception cref="Exception"></exception>
        protected string SameVersionCPArgs()
        {
            var target = Owner.FindVersion(_version, GameType.Ordinary);
            if (target == null) { 
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version"); 
            }
            return target.GetCPArgs();
        }

        /// <summary>
        /// Decompress The Native Libraries
        /// </summary>
        public virtual async Task Decompress()
        {
            foreach(LibraryBase library in libraries)
            {
                if (library.LibraryType != LibraryType.Native) continue;
                await Task.Run(
                    () => { 
                        (library as NativeLibrary??new NativeLibrary()).Extract( NativeDir); 
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
        #endregion

        #region Others
        /// <summary>
        /// Get The Libraries Array
        /// </summary>
        /// <returns>Array</returns>
        public virtual LibraryBase[] GetLibraries()
        {
            return libraries.ToArray();
        }

        /// <summary>
        /// /// Run <c>GetLibraries()</c> For The Same Version Game Inside The Folder
        /// </summary>
        /// <returns>The Array</returns>
        /// <exception cref="Exception"></exception>
        protected List<LibraryBase> SameVersionLibraries()
        {
            var target = Owner.FindVersion(_version, GameType.Ordinary);
            if (target == null)
            {
                Logger.Error($"Could Not Find {_version}"); throw new Exception("Could Not Find Version");
            }
            return target.libraries;
        }
        #endregion

        /// <summary>
        /// Close the game
        /// </summary>
        public virtual void Close()
        {
            FileStream fs;
            StreamWriter sw;
            string settingpath = Localize.PathLocalize(SettingsPath);
            Logger.Info($"{Path} Closing");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingpath)?? "");
            fs = new(settingpath, FileMode.Create);
            sw = new(fs);
            if (Settings != null) sw.Write(Settings.ToString());
            sw.Close();
            fs.Close();
            Logger.Info($"{Path} Closed. File Saved");
        }
    }

    /// <summary>
    /// Different types of Minecraft
    /// Mod:ModLoader like Fabric, Quilt, Liteloader, Forge and NeoForge
    /// Optifine:Only With Optifine
    /// Ordinary:No modloader or Optifine.
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// Ordinary Game
        /// </summary>
        Ordinary,
        /// <summary>
        /// Optifine Game
        /// </summary>
        Optifine,
        /// <summary>
        /// Mod Game
        /// </summary>
        Mod
    }
}
