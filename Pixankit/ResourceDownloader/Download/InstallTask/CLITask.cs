using PixanKit.ResourceDownloader.Tasks;
using System.Diagnostics;
using PixanKit.LaunchCore.Logger;

namespace PixanKit.ResourceDownloader.Download.InstallTask;

/// <summary>
/// Command Language Running Task
/// </summary>
public class CliTask : ProgressTask
{
    private readonly Process _process;

    StreamReader Output => _process.StandardOutput;

    /// <summary>
    /// Inits the instance with the file path and the arguments
    /// </summary>
    /// <param name="file">file path</param>
    /// <param name="args">arguments</param>
    public CliTask(string file, string args)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true
        };
        _process = new Process()
        {
            StartInfo = startInfo
        };
    }

    /// <summary>
    /// Inits the instance with the file path, the arguments and the running dir
    /// </summary>
    /// <param name="file">the exact path of the file</param>
    /// <param name="args">the arguments</param>
    /// <param name="workingDir">the directory where the process is expected to run</param>
    public CliTask(string file, string args, string workingDir)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            WorkingDirectory = workingDir
        };
        _process = new Process()
        {
            StartInfo = startInfo
        };
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task Running()
    {
        _process.Start();
        while (!_process.HasExited)
            Logger.Info("PixanKit.ResourceDownloader", 
                $"Process: " +
                         $"{await Output.ReadLineAsync()}");
        await base.Running();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <exception cref="InvalidOperationException"><inheritdoc/></exception>
    public override void Cancel()
    {
        if (_status >= ProgressStatus.Canceled) throw new InvalidOperationException();
        _process.Kill();
        base.Cancel();
    }
}