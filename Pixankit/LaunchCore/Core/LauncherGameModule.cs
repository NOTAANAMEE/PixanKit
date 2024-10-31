using PixanKit.LaunchCore.GameModule.Exceptions;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.Mod;
using System.Runtime.CompilerServices;
using PixanKit.LaunchCore.SystemInf;
using PixanKit.LaunchCore.Extention;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Log;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// The folders
        /// </summary>
        public Folder[] Folders
        {
            get => _folders.ToArray();
        }

        /// <summary>
        /// The default game to be launched
        /// </summary>
        public GameBase? TargetGame { get; set; }

        /// <summary>
        /// Mod Informations. Mod Cache
        /// </summary>
        public ModBase[] Mods
        {
            get => _modCache.Values.ToArray();
        }

        /// <summary>
        /// Game Cache For Mods
        /// </summary>
        public JObject GameModCache = new();

        private List<Folder> _folders = new();

        private Dictionary<string, ModBase> _modCache = new();

        private bool nogame;//Judge whether there is a game. It will be changed automatically  

        /// <summary>
        /// Launch the game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="DependencyException"></exception>
        public ProcessResult Launch(GameBase game)
        {
            game.LaunchCheck();
            long timeStamp = DateTime.Now.Ticks;
            string cmd = GenerateLaunchCmd(game);
            JavaRuntime? java = ChooseRuntime(game);
            cmd = PlayerInLine(cmd);
            cmd = Localize.PathLocalize(cmd);
            string pth = game.Path[game.Path.LastIndexOf(".minecraft/versions/")..];
            cmd = $"-Xmx{Initors.GetMemory()}m " + cmd;
            cmd = cmd.Replace("${launcher_name}", LauncherName);
            cmd = cmd.Replace("${launcher_version}", VersionName);
            if (java == null) throw new NullReferenceException();
            //game.Decompress().Wait();
            Logger.Info("Game Arg Generated Successfully. Stored in a.bat");
            //int a = p.ExitCode;
            FileStream fs = new("./a.bat", FileMode.Create);
            StreamWriter sw = new(fs);
            sw.Write("\"" + java.JavaEXE + "\" " + cmd);
            sw.Close();
            fs.Close();
            game.Decompress().Wait();
            ProcessStartInfo info = new()
            {
                CreateNoWindow = true,
                FileName = java.JavaEXE,
                Arguments = cmd,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = Localize.PathLocalize(Files.LauncherConfigDir),
            };
            Logger.Info("Game Launched");
            Process? p = Process.Start(info);
            MemoryStream ms = new();
            sw = new(ms);
            while (!p.HasExited)
            {
                sw.Write(p.StandardOutput.ReadToEnd());
            }
            sw.Write(p.StandardOutput.ReadToEnd());
            DateTime now = DateTime.Now;
            Logger.Info("Game Exited");
            if (p.ExitCode != 0) Logger.Warn("Error happened to the game!");
            Dictionary<long, string> files = GetTimestampAndFilePath(game.CrashReportDir);
            return new ProcessResult()
            {
                ReturnCode = p.ExitCode,
                //Successful = p.ExitCode == 0,
                LogGZPath = GetTimestampAndFilePath(Files.LauncherConfigDir + "/logs").Last().Value,
                OutputStream = ms,
                CrashFilePath = files.ContainsKey(now.Ticks) ? null : files[now.Ticks]
            };
        }

        /// <summary>
        /// Launch the default game
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public ProcessResult Launch()
        {
            if (TargetGame == null) throw new NullReferenceException();
            return Launch(TargetGame);
        }

        /// <summary>
        /// Add a folder to the Launcher
        /// </summary>
        /// <param name="folder"></param>
        /// <exception cref="HasAddedException"></exception>
        public void AddFolder(Folder folder)
        {
            foreach(Folder f in _folders)
            {
                if (f.Path == folder.Path) throw new HasAddedException("Folder has added before");
            }
            _folders.Add(folder);
            if (_folders.Count > 0) nogame = false;            
            ResetTargetGame();
        }

        /// <summary>
        /// Remove the folder.
        /// </summary>
        /// <param name="folder"></param>
        public void RemoveFolder(Folder folder) 
        {
            RemoveFolder(folder.Path);
        }

        /// <summary>
        /// Remove the foder with specific path
        /// </summary>
        /// <param name="path"></param>
        public void RemoveFolder(string path)
        {
            foreach (Folder f in _folders)
            {
                if (f.Path == path)
                {
                    _folders.Remove(f);
                    if (TargetGame == null || !f.HasGame(TargetGame)) return;
                    TargetGame = null;
                    ResetTargetGame();
                }
            }
        }

        /// <summary>
        /// Returns the folder with specific path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Folder? FindFolder(string path)
        {
            foreach(Folder folder in _folders)
            {
                if (folder.Path == path) return folder;
            }
            return null;
        }

        /// <summary>
        /// Add the game to the folder. This method will automatically judge the folder.
        /// </summary>
        /// <param name="game"></param>
        /// <exception cref="NoFolderException"></exception>
        public void AddGame(GameBase game)
        {
            foreach(Folder f in _folders)
            {
                if (game.GameFolder.StartsWith(f.Path))
                {
                    f.InternalAddGame(game);
                    nogame = false;
                    ResetTargetGame();
                    return;
                }
            }
            throw new NoFolderException("Folder Not Found");
        }

        /// <summary>
        /// Remove the game from the folder. This method will automatically judge the folder.
        /// </summary>
        /// <param name="game"></param>
        public void RemoveGame(GameBase game)
        {
            game.Owner.InternalRemoveGame(game);
            if (TargetGame == game) ResetTargetGame();
        }

        /// <summary>
        /// Get The Game From Its Path
        /// </summary>
        /// <param name="path">The Dir Of The Game</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GameBase? FindGame(string path)
        {
            string folderpath = path.Remove(path.LastIndexOf("/versions/"));
            string? name = Path.GetDirectoryName(path) ?? throw new ArgumentException(path);
            return FindFolder(folderpath).FindGame(name);
        }

        /// <summary>
        /// Add A Mod Information
        /// </summary>
        /// <param name="mod"></param>
        public void AddMod(ModBase mod)
        {
            if (_modCache.ContainsKey(mod.ID)) return;
            _modCache.Add(mod.ID, mod);
        }

        /// <summary>
        /// Remove A Mod Information
        /// </summary>
        /// <param name="mod"></param>
        public void RemoveMod(ModBase mod)
        {
            _modCache.Remove(mod.ID);
        }

        /// <summary>
        /// Set the information of the file by ID and the mod information count will add 1
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="Exception"></exception>
        public void SetModInformation(ModFile file)
        {
            string index = file.ID;
            if (!_modCache.ContainsKey(index)) throw new Exception($"Could not find the id:{file.ID}");
            _modCache[index].SetModInformation(file);
        }

        /// <summary>
        /// Set the information of the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ModInf"></param>
        public void SetModInformation(ModFile file, ModBase ModInf)
        {
            file.ID = ModInf.ID;
            ModInf.SetModInformation(file);
        }

        /// <summary>
        /// Move game to another folder. Not implemented yet
        /// </summary>
        /// <param name="game"></param>
        /// <param name="folder"></param>
        public static void MoveGame(GameBase game, Folder folder)
        {
            if (folder.HasGame(game)) return;
            game.folder.RemoveGame(game);
            folder.AddGame(game);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate The Launch Command
        /// </summary>
        /// <param name="game">target gae</param>
        /// <returns>command</returns>
        public string GenerateLaunchCmd(GameBase game)
        {
            string cmd = game.GetLaunchArgument();
            cmd = cmd.Replace("${arguments}", Settings["argument"].ToString());

            return cmd;
        }

        /// <summary>
        /// Get The Mod ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ModBase FindMod(string id)
            => _modCache[id];

        private void ResetTargetGame()
        {
            if (TargetGame != null || nogame) return;
            if (_folders.Count > 0 )
            {
                foreach (Folder folder in _folders) 
                {
                    if (folder.Count > 0)
                    {
                        TargetGame = folder.First;
                        return;
                    }
                }
            }
            nogame = true;
        }

        private static Dictionary<long, string> GetTimestampAndFilePath(string dir)
        {
            string[] files = Directory.GetFiles(Localize.PathLocalize(dir));
            Dictionary<long, string> keyValuePairs = new();
            foreach (string file in files) 
            {
                long time = File.GetCreationTime(file).Ticks;
                keyValuePairs.Add(time, file);
            }
            return keyValuePairs;
        }
    }
}
