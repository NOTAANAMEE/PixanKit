using PixanKit.ResourceDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.FuncTask
{
    public delegate Task TaskAction();

    public delegate Task<T> TaskFunc<T>();

    public class ActionTask: ProcessTask
    {
        public TaskAction Action;

        protected override async Task Running()
        {
            await Action();
            await base.Running();
        }
    }

    public class FuncTask<T>: ProcessTask
    {
        public T Return;

        public TaskFunc<T> Function;

        protected override async Task Running()
        {
            Return = await Function();
            await base.Running();
        }
    }
}
