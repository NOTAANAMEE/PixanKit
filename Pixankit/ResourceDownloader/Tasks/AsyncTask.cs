using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks
{
    /// <summary>
    /// Start several tasks at the same time.
    /// Override Method: Start
    /// Start the tasks at the same time
    /// </summary>
    public abstract class AsyncTask: ProcessTask
    {
        /// <summary>
        /// Initialize
        /// </summary>
        protected AsyncTask():base() { }

        /// <summary>
        /// Start The Task
        /// </summary>
        public override void Start()
        {
            base.Start();
            foreach (var task in Tasks)
            {
                task.Start();
            }
            _ = Running();
        }
    }
}
