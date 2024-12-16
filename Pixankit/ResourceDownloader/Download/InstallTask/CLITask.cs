using PixanKit.ResourceDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

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
        /// Initor
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
        /// <inheritdoc/>
        /// </summary>
        protected override async Task Running()
        {
            process.Start();
            while (!process.HasExited)
                Console.WriteLine($"Process Output: {Output.ReadLine()}");
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
