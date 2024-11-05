using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks
{
    /// <summary>
    /// Start next task after this task finishes
    /// Override Method: Running
    /// await the first one and start the next one.
    /// </summary>
    public class SequenceTask: ProcessTask
    {
        /// <summary>
        /// Initor Add Tasks Later
        /// </summary>
        public SequenceTask() : base() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override async Task Running()
        {
            foreach (var task in Tasks) 
            {
                if (Status == ProcessStatus.Canceling) return;
                task.Start();
                await task;
            }
            _ = base.Running();
        }
    }
}
