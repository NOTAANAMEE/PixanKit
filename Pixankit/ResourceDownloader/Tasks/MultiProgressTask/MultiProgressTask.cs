using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.MultiProgressTask
{
    public class MultiProgressTask:ProgressTask
    {
        public List<ProgressTask> ProgressTasks = new();        

        public void Add(ProgressTask task)
        {
            if (_status > ProgressStatus.Inited) 
                throw new InvalidOperationException("Add A Process Before The Task Starts");
            ProgressTasks.Add(task);
            _tasks.Add(task.MainTask);
            task.OnException += this.OnException;
            task.OnReport += Report;
        }

        /*public override void Start()
        {
            foreach (var progressTask in ProgressTasks)
            {
                progressTask.Start();
            }
            base.Start();
        }*/

        public override void Cancel()
        {
            foreach (var progressTask in ProgressTasks)
            {
                progressTask.Cancel();
            }
            //Remove();
            base.Cancel();
        }

        protected override void Finish()
        {
            //Remove();
            base.Finish();
        }

        protected override void Report(double progress)
        {
            progress = 0;
            foreach (var task in ProgressTasks) 
            {
                progress += task.Progress;
            }
            progress /=  ProgressTasks.Count;
            base.Report(progress);
        }
    }
}
