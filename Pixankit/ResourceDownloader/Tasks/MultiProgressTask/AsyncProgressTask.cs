using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.MultiProgressTask
{
    public class AsyncProgressTask:MultiProgressTask
    {
        public override void Start()
        {
            if (_status == ProgressStatus.Inited) 
            foreach (var item in ProgressTasks) 
            { 
                item.Start();
            }
            base.Start();
        }
    }
}
