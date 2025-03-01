using PixanKit.LaunchCore.Log;
using PixanKit.ResourceDownloader.Tasks;
using System.Diagnostics;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    /// <summary>
    /// Command Language Running Task
    /// </summary>
    public class CLITask : ProgressTask
    {

        ProcessStartInfo StartInfo;

        Process process;

        StreamReader Output { get => process.StandardOutput; }

        /// <summary>
        /// Inits the instance with the file path and the arguments
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="args">arguments</param>
        public CLITask(string file, string args)
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = args,
                RedirectStandardOutput = true
            };
            process = new Process()
            {
                StartInfo = StartInfo
            };
        }

        /// <summary>
        /// Inits the instance with the file path, the arguments and the running dir
        /// </summary>
        /// <param name="file">the exact path of the file</param>
        /// <param name="args">the arguments</param>
        /// <param name="workingdirectory">the directory where the process is expected to run</param>
        public CLITask(string file, string args, string workingdirectory)
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = args,
                RedirectStandardOutput = true,
                WorkingDirectory = workingdirectory
            };
            process = new Process()
            { 
                StartInfo = StartInfo
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task Running()
        {
            process.Start();
            while (!process.HasExited)
                Logger.Info("PixanKit.ResourceDownloader", $"Porcess: {Output.ReadLine()}");
            await base.Running();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="InvalidOperationException"><inheritdoc/></exception>
        public override void Cancel()
        {
            if (_status >= ProgressStatus.Canceled) throw new InvalidOperationException();
            process.Kill();
            base.Cancel();
        }
    }
}
