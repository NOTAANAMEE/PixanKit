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
    public class GameBase
    {
        /// <summary>
        /// Minecraft Game Name
        /// </summary>
        public string Name 
        { 
            get => System.IO.Path.GetDirectoryName(_path) ?? ""; 
        }

        /// <summary>
        /// Minecraft Game Description
        /// </summary>
        public string Description 
        { 
            get => Settings["description"].ToString();
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
            get => (folder == null)? RootDir + "/libraries"
                :folder.LibraryDir;
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
        public string RootDir
        {
            get => _path.Remove(_path.LastIndexOf("/versions/"));
        }

        /// <summary>
        /// The place to get Minecraft Version
        /// </summary>
        public string Version 
        { 
            get => _version; 
        }        

        /// <summary>
        /// The place to store native binary libraries
        /// </summary>
        public string NativeDir 
        { 
            get => _path + $"/{Name}-natives"; 
        }

        /// <summary>
        /// The Settings Config Path
        /// </summary>
        public string SettingsPath
        {get => _path + Files.GameSettingName; }

        /// <summary>
        /// The Actual path of crash report. tar.gz file
        /// </summary>
        public string CrashReportDir
        {
            get => RunningDir + "/crash-reports";
        }

        /// <summary>
        /// Get The Type Of Game
        /// </summary>
        public GameType GameType { get => _gameType; }

        /// <summary>
        /// Settings
        /// </summary>
        public JObject? Settings = new()
        {
            { "java", "overall"},//"overall": the same as the overall settings, "specified": Should be the same version, "closest": The closest version(Bigger / equal), "newest": The largest version, default: user specified
            { "argument", "overall" },//"overall": the same as the overall settings, default:user specified
            { "runningfolder", "self" }, //"overall":the same as the overall settings, "self": self folder defult: user specified
            { "description", "A Minecraft Game" }
        };

        public JObject Jtmpdata = new();

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

        /// <summary>
        /// The Initor
        /// </summary>
        /// <param name="path">The version path. Like "C:/Users/Admin/AppData/Roaming/.minecraft/versions/1.20.1"</param>
        /// <param name="jData">The JSON file.</param>
        public GameBase(string path, JObject jData)
        {
            _path = path;
            Jtmpdata = jData;
            //Set the data by JSON
            className = jData["mainClass"].ToString();
            SetLibrary(jData);
            SetGameArgument(jData);
            SetJVMArguments(jData);

            //Set The Version. Real Version
            if (jData["inheritsfrom"] != null) _version = jData["inheritsfrom"].ToString();
            else _version = jData["id"].ToString();
            releaseType = jData["type"].ToString();

            SetSettings();
            //Log
            Logger.Info($"Game Base Added. Path:{_path}");
        }

        protected GameBase(string path) 
        {
            _path = path;

            //This is for reading the JSON file
            JObject jData;
            {
                string tmpcontent = File.ReadAllText(path);
                jData = JObject.Parse(tmpcontent);
            }
            
            //Set the data by JSON
            className = jData["mainClass"].ToString();
            SetLibrary(jData);
            SetGameArgument(jData);
            SetJVMArguments(jData);
            releaseType = jData["type"].ToString();
            //Set The Version. Real Version
            if (jData["inheritsfrom"] != null) _version = jData["inheritsfrom"].ToString();
            else _version = jData["id"].ToString();

            SetSettings();
            //Log
            Logger.Info($"Game Base Added. Path:{_path}");
        }

        protected GameBase(JObject jData)
        {
            Jtmpdata = jData;
            //Set the data by JSON
            className = jData["mainClass"].ToString();
            SetLibrary(jData);
            SetGameArgument(jData);
            SetJVMArguments(jData);
            releaseType = jData["type"].ToString();
            //Set The Version. Real Version
            _version = (jData["inheritsfrom"] != null)? jData["inheritsfrom"].ToString():jData["id"].ToString();

            //Log
            Logger.Info($"Game Base Added. Path:{_path} Type{GetType()}");
        }

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
            switch (Settings["argument"].ToString())
            {
                case "overall":
                    arg = "${arguments} " + arg;
                    break;
                default:
                    arg = Settings["arguments"].ToString() + " " + arg; 
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
        /// Set the folder of the game
        /// </summary>
        /// <param name="owner">folder. Should be the actual folder that it is in</param>
        public virtual void SetOwner(Folder owner)
        {
            if (_path.StartsWith(owner.Path))
            folder = owner;
            //Launcher.GameInit?.Invoke(Owner.Owner, this);
        }

        private void SetSettings()
        {
            if (File.Exists(Localize.PathLocalize(SettingsPath)))
                Settings = JObject.Parse(File.ReadAllText( _path + Files.GameSettingName));
        }

        private void SetLibrary(JObject jData)
        {
            foreach (JToken token in jData["libraries"])
            {
                LibraryBase? tmp = null;

                if (!LibraryBase.GetAllowedSystem(token).Contains(SystemInformation.OSName)) continue;//不支持就下一个

                if (folder != null && (tmp = folder.FindLibrary(token["name"].ToString())) != null)
                {
                    libraries.Add(tmp);
                    continue;
                }

                switch (LibraryBase.GetLibraryType(token))
                {
                    case LibraryType.Ordinary:
                        libraries.Add(tmp = new OrdinaryLibrary(token));
                        break;
                    case LibraryType.Native:
                        libraries.Add(new NativeLibrary(token));
                        if ((token["downloads"] as JObject).Count > 1) libraries.Add(tmp = new OrdinaryLibrary(token));
                        break;
                    case LibraryType.Mod:
                        libraries.Add(tmp = new LoaderLibrary(token));
                        break;
                }
                if (folder != null && tmp != null) folder.AddLibrary(tmp);
            }
            Logger.Info($"Libraries Added. Number:{libraries.Count}");
        }

        internal virtual void SetGameArgument(JObject jData)
        {
            if (jData["minecraftArguments"] != null) 
            {
                gameArguments = jData["minecraftArguments"].ToString();
            }
            else
            {
                foreach(JToken token in jData["arguments"]["game"])
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
            if (jData["arguments"] != null && jData["arguments"]["jvm"] != null)
            {
                jvmargArray = jData["arguments"]["jvm"] as JArray;
            }
            else
            {
                jvmargArray = JArray.Parse(tmp);
            }
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
                    if (token["value"].Type == JTokenType.String) tmpstr = token["value"].ToString();
                    else tmpstr = string.Join(' ', token["value"] as JArray);
                    javaArguments += tmpstr + " ";
                }
            }
        }

        internal virtual string GetJVMArguments()
            => javaArguments;

        internal virtual string GetGameArguments()
            => gameArguments;

        internal virtual string GetAssetsID()
            => assetsID;

        private string GetRunningFolder()
        {
            string runningfolder = Settings["runningfolder"].ToString();
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

        internal virtual string GetCPArgs()
        {
            string classpath = "";
            foreach (LibraryBase library in libraries) 
            {
                classpath += LibraryDir + "/" + library.Path + Localize.LocalParser;
            }
            classpath += Path;
            return classpath;
        }

        /// <summary>
        /// Decompress The Native Libraries
        /// </summary>
        /// <returns></returns>
        public virtual async Task Decompress()
        {
            foreach(LibraryBase library in libraries)
            {
                if (library.LibraryType != LibraryType.Native) continue;
                await Task.Run(
                            () => { (library as NativeLibrary).Extract(LibraryDir, NativeDir); }
                            );
            }
        }

        /// <summary>
        /// Close the game
        /// </summary>
        public virtual void Close()
        {
            FileStream fs = new(Localize.PathLocalize(SettingsPath), FileMode.Create);
            StreamWriter sw = new(fs);
            sw.Write(Settings.ToString());
            sw.Close();
            fs.Close();
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
        Ordinary,
        Optifine,
        Mod
    }
}
