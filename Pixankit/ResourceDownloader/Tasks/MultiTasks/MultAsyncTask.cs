using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.MultiTasks
{
    /// <summary>
    /// Run multi processes at the same time.<br/>
    /// Override Methods: <br/><see cref="Start"/>
    /// </summary>
    public class MultiAsyncTask: MultiTask
    {
        /// <summary>
        /// Initor
        /// </summary>
        public MultiAsyncTask():base() { }

        /// <summary>
        /// Starts The Process
        /// </summary>
        public override void Start()
        {
            foreach (var process in processes) 
            {
                process.Start();
            }
            base.Start();
        }
    }
}
