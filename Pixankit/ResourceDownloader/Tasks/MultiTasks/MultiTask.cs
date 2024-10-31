using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.MultiTasks
{
    /// <summary>
    /// Multi Process Class
    /// </summary>
    public abstract class MultiTask : ProcessTask
    {
        /// <summary>
        /// This list stores the proccesses in the MultiTask process
        /// </summary>
        public List<ProcessTask> processes = new();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override double Schedule
        {
            get
            {
                double tmp = 0;
                foreach (ProcessTask task in processes) tmp += task.Schedule;
                return tmp / processes.Count;
            }
        }

        /// <summary>
        /// Init A MultiTask
        /// </summary>
        protected MultiTask() : base() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override async Task Running()
        {
            foreach (var process in processes)
            {
                if (_status != ProcessStatus.Running)process.Cancel();
                await process.MainTask;
            }
            TaskStopped.SetResult(true);
            if (Status == ProcessStatus.Running) Finish();
        }

        /// <summary>
        /// Add a process to the MultiTask process
        /// </summary>
        /// <param name="task"></param>
        public virtual void Add(ProcessTask task)
        {
            processes.Add(task);
            Tasks.Add(task.MainTask);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Cancel()
        {
            if (Status == ProcessStatus.Running)
            {
                foreach (var process in processes)
                {
                    if (process.Status < ProcessStatus.Canceling) process.Cancel();
                }
            }
            base.Cancel();
        }
    }
}
