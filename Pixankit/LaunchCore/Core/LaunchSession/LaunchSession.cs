using PixanKit.LaunchCore.Extension;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.JavaModule.Java;
using PixanKit.LaunchCore.SystemInf;
using System.Diagnostics;

namespace PixanKit.LaunchCore.Core.LaunchSession;

/// <summary>
/// Represents a session for launching a Minecraft game instance.
/// </summary>
public class LaunchSession
{
    /// <summary>
    /// Gets the path to the log directory. Java process will run under <see cref="Files.CacheDir"/>.
    /// </summary>
    public static string LogPath => Files.CacheDir + "logs";

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
    private readonly ProcessStartInfo _startInfo;
        
    private readonly string _preArgs;
        
    private readonly string _postArgs;
        
    private readonly List<KeyValuePair<string, string>> _variables;

    DateTime _time = DateTime.Now;

    private string _logPath = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="LaunchSession"/> class.
    /// </summary>
    /// <param name="game">The game instance to be launched.</param>
    /// <param name="java">The Java runtime used for launching.</param>
    /// <param name="args">The command-line arguments for the process.</param>
    public LaunchSession(GameBase game, JavaRuntime java, string args):
        this(game, java, "", args ,"",[]){}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <param name="java"></param>
    /// <param name="preArgs"></param>
    /// <param name="args"></param>
    /// <param name="postArgs"></param>
    /// <param name="env"></param>
    public LaunchSession(GameBase game, JavaRuntime java, 
        string preArgs, string args, string postArgs,
        List<KeyValuePair<string, string>> env)
    {
        Game = game;
        Runtime = java;
        Arguments = args;
        _preArgs = preArgs;
        _postArgs = postArgs;
        _variables = env;
        _startInfo = new()
        {
            FileName = SysInfo.Shell(),
            WorkingDirectory = Path.GetFullPath(Files.CacheDir),
            CreateNoWindow = true,
            RedirectStandardInput = true,
        };
    }


    /// <summary>
    /// Starts the game process.
    /// </summary>
    public void Start()
    {
        Process = new()
        {
            StartInfo = _startInfo,
            EnableRaisingEvents = true,
        };
        _time = DateTime.Now;
        Process.Start();
        Process.Exited += Exit;
            
        foreach (var pair in _variables)
            Process.StandardInput.WriteLine(SysInfo.GetVarCmd(pair.Key, pair.Value));
            
        Process.StandardInput.WriteLine(
            $"{_preArgs} \"{Runtime.JavaExe}\" {Arguments} {_postArgs}");
        Process.StandardInput.WriteLine("exit");
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
        
    }

    /// <summary>
    /// Gets the result of the game launch.
    /// </summary>
    /// <returns>The game result</returns>
    public ProcessResult GetResult()
    {
        if (Process?.HasExited ?? true) return new();
        _logPath = Directory.GetDirectories(LogPath)
               .Where(str => str.EndsWith(".gz"))
               .FirstOrDefault(str =>
                   Directory.GetCreationTime(str) == _time)
                   ?? "";
        return new()
        {
            ReturnCode = Process?.ExitCode ?? -1,
            LogGzPath = _logPath,
        };
    }
}