using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.MultiTasks
{
    /// <summary>
    /// Run Tasks In Sequence
    /// </summary>
    public class MultiSequenceTask:MultiTask
    {
        /// <summary>
        /// Init a MultiSequence Task
        /// </summary>
        public MultiSequenceTask() : base() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Cancel()
        {
            foreach (var process in processes) 
            {
                if (process.Status < ProcessStatus.Canceling) process.Cancel();
            }
            base.Cancel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override async Task Running()
        {
            foreach (var process in processes) 
            {
                if (Status == ProcessStatus.Canceled) return; 
                process.Start();
                await process.MainTask;
            }
            TaskStopped.SetResult(true);
            if (Status == ProcessStatus.Running) Finish();
        }
    }
}
