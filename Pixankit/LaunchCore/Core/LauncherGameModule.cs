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
using System.Runtime.CompilerServices;
using PixanKit.LaunchCore.SystemInf;
using PixanKit.LaunchCore.Extention;
using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Log;
using System.Buffers;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Gets the collection of folders managed by the launcher.
        /// </summary>
        public Folder[] Folders
        {
            get => _folders.ToArray();
        }

        /// <summary>
        /// Gets or sets the default game to be launched.
        /// </summary>
        public GameBase? TargetGame { get; set; }

        private List<Folder> _folders = [];

        /// <summary>
        /// Launch the game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public ProcessResult Launch(GameBase game)
        {
            string cmd = InlineCommand(game);


            JavaRuntime? java = ChooseRuntime(game);
            if (java == null) throw new NullReferenceException();


            Logger.Info("Game Arg Generated Successfully. Stored in a.bat");
            FileStream fs = new("./a.bat", FileMode.Create);
            StreamWriter sw = new(fs);
            sw.Write("\"" + java.JavaEXE + "\" " + cmd);
            sw.Close();
            fs.Close();


            game.Decompress().Wait();
            string runningdir = 
                string.Concat(AppDomain.CurrentDomain.BaseDirectory, 
                Localize.PathLocalize(Files.ConfigDir[2..]));

            ProcessStartInfo info = new()
            {
                CreateNoWindow = true,
                FileName = java.JavaEXE,
                Arguments = cmd,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = runningdir
            };
            Logger.Info("Game Launched");
            Process? p = Process.Start(info);
            if (p == null) return new ProcessResult();
            MemoryStream ms = new();
            sw = new(ms);
            while (!p.HasExited)
            {
                sw.Write(p.StandardOutput.ReadToEnd());
            }
            sw.Write(p.StandardOutput.ReadToEnd());


            DateTime now = DateTime.Now;
            Logger.Info($"Game Exited with code {p.ExitCode}");
            if (p.ExitCode != 0) Logger.Warn("Error happened to the game!");
            ms.Position = 0;


            Dictionary<long, string> files;
            try
            {
                files = GetTimestampAndFilePath(game.CrashReportDir);
            }
            catch { files = new Dictionary<long, string>(); }
            return new ProcessResult()
            {
                ReturnCode = p.ExitCode,
                LogGZPath = "",//GetTimestampAndFilePath(runningdir + "/logs").Last().Value,
                OutputStream = ms,
                CrashFilePath = "",
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

        private string InlineCommand(GameBase game)
        {
            game.LaunchCheck();
            long timeStamp = DateTime.Now.Ticks;
            string cmd = GenerateCommand(game);

            cmd = PlayerInLine(cmd);
            cmd = Localize.PathLocalize(cmd);
            string pth = game.Path[game.Path.LastIndexOf(".minecraft/versions/")..];
            cmd = $"-Xmx{Initors.GetMemory()}m " + cmd;
            cmd = cmd.Replace("${launcher_name}", LauncherName);
            cmd = cmd.Replace("${launcher_version}", VersionName);
            return cmd;
        }

        /// <summary>
        /// Checks whether the game is one of the games that exists in any foldr
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool Contains(GameBase? game)
        {
            if (game == null) return false;
            return Contains(game.Owner) && (game.Owner?.Contains(game) ?? false);
        }

        /// <summary>
        /// Checks whether the folder is in the folder collection
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool Contains(Folder? folder)
        {
            if(folder == null) return false;
            return _folders.Contains(folder);
        }

        /// <summary>
        /// Add a folder to the Launcher
        /// </summary>
        /// <param name="folder"></param>
        /// <exception cref="InvalidOperationException"> Do not add a folder which is added</exception>
        public void AddFolder(Folder folder)
        {
            foreach(Folder f in _folders)
            {
                if (f.Path == folder.Path) throw new InvalidOperationException("Folder has added before");
            }
            _folders.Add(folder);
            folder.SetOwner(this);
            FolderAdd?.Invoke(folder);
            UpdateTargetGame();
            Logger.Info($"Folder {folder.Path} Added");
        }

        /// <summary>
        /// Remove the folder.
        /// </summary>
        /// <param name="folder"></param>
        public void RemoveFolder(Folder folder) 
        {
            if (!_folders.Contains(folder)) return;
            _folders.Remove(folder);
            FolderRemove?.Invoke(folder);
            UpdateTargetGame();
        }

        /// <summary>
        /// Remove the foder with specific path
        /// </summary>
        /// <param name="path"></param>
        public void RemoveFolder(string path)
        {
            Folder? f = FindFolder(path);
            if (f != null) RemoveFolder(f);
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
                    GameAdd?.Invoke(game);
                    UpdateTargetGame();
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
            if (game.Owner == null) return;
            game.Owner.InternalRemoveGame(game);

            UpdateTargetGame();
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
            var res = FindFolder(folderpath);
            if (res == null) return null;
            return res.FindGame(name);
        }

        /// <summary>
        /// Generate The Launch Command
        /// </summary>
        /// <param name="game">target gae</param>
        /// <returns>command</returns>
        public string GenerateCommand(GameBase game)
        {
            string cmd = game.GetLaunchArgument();
            cmd = cmd.Replace("${arguments}", (Settings["arguments"] ?? new JObject()).ToString());

            return cmd;
        }

        private void UpdateTargetGame()
        {
            if (TargetGame is null) FirstGame();
            else if (TargetGame.Owner is null) throw new Exception();
            else if (!_folders.Contains(TargetGame.Owner)) FirstGame();
            else if (TargetGame.Owner.Contains(TargetGame)) return;
            else if (TargetGame.Owner.Count > 0) TargetGame = TargetGame.Owner.First;
            else FirstGame();
            TargetGameChange?.Invoke(TargetGame);
        }

        private void FirstGame()
        {
            foreach (var folder in _folders) if(folder.Count > 0)
            {
                TargetGame = folder.First;
                return;
            }
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
