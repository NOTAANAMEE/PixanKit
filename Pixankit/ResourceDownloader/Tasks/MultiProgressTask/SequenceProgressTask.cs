using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.MultiProgressTask
{
    public class SequenceProgressTask:MultiProgressTask
    {

        protected override async Task Running()
        {
            foreach (var progresstask in ProgressTasks)
            {
                if (CancellationToken.Token.IsCancellationRequested) break;
                progresstask.Start();
                await progresstask.MainTask;
                //progresstask.OnReport -= Report;
            }

            await base.Running();
        }
    }
}
