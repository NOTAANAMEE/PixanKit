using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using System.Diagnostics;

namespace PixanKit.LaunchCore.Core
{
    /// <summary>
    /// Represents a session for launching a Minecraft game instance.
    /// </summary>
    public class LaunchSession
    {
        /// <summary>
        /// Gets the path to the log directory. Java process will run under <see cref="Files.CacheDir"/>.
        /// </summary>
        public static string LogPath { get => Files.CacheDir + "/logs"; }

        /// <summary>
        /// Gets the process associated with the game launch.
        /// </summary>
        public Process? Process { get; private set; }

        /// <summary>
        /// Gets the game instance being launched.
        /// </summary>
        public GameBase Game { get; private set; }

        /// <summary>
        /// Gets the arguments used to launch the game.
        /// </summary>
        public string Arguments { get; private set; }

        /// <summary>
        /// Gets the Java runtime used to launch the game.
        /// </summary>
        public JavaRuntime Runtime { get; private set; }

        /// <summary>
        /// Stores the process start information.
        /// </summary>
        private readonly ProcessStartInfo StartInfo;

        DateTime time = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchSession"/> class.
        /// </summary>
        /// <param name="game">The game instance to be launched.</param>
        /// <param name="java">The Java runtime used for launching.</param>
        /// <param name="args">The command-line arguments for the process.</param>
        public LaunchSession(GameBase game, JavaRuntime java, string args)
        {
            Game = game;
            Runtime = java;
            Arguments = args;
            StartInfo = new()
            {
                FileName = java.JavawEXE,
                Arguments = args,
                WorkingDirectory =
                    string.Concat(AppDomain.CurrentDomain.BaseDirectory, Files.CacheDir),
                CreateNoWindow = true,
            };
        }

        /// <summary>
        /// Starts the game process.
        /// </summary>
        public void Start()
        {
            Process = new()
            {
                StartInfo = StartInfo,
                EnableRaisingEvents = true,
            };
            Process.Start();
            Process.Exited += Exit;
        }

        /// <summary>
        /// Stops the running game process.
        /// </summary>
        public void Stop()
        {
            if (Process?.HasExited ?? true) return;
            Process.Kill();
            Process.WaitForExit();
        }

        /// <summary>
        /// Waits for the game process to exit.
        /// </summary>
        public void WaitForExit()
        {
            if (Process == null) Start();
            Process?.WaitForExit();
        }

        private void Exit(object? sender, EventArgs e)
        {
            time = DateTime.Now;
        }

        /// <summary>
        /// Gets the result of the game launch.
        /// </summary>
        /// <returns>The game result</returns>
        public ProcessResult GetResult()
        {
            if (Process?.HasExited ?? true) return new();
            string path = "";
            foreach (string dir in Directory.GetDirectories(Files.CacheDir + "/logs"))
            {
                if (Directory.GetCreationTime(dir) == time)
                {
                    path = dir;
                    break;
                }
            }
            return new()
            {
                ReturnCode = Process?.ExitCode ?? -1,
                LogGZPath = path,
                CrashFilePath = LogPath,
            };
        }
    }
}
