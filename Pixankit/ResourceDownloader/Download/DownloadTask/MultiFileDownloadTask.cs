using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.DownloadTask
{
    public class MultiFileDownloadTask: AsyncProgressTask
    {
        public static int ThreadNum = 64;

        protected string[] _url;

        protected string[] _fileName;

        protected int threadnum = 1;

        public MultiFileDownloadTask(string[] url, string[] path) : this(url, path, ThreadNum)
        { }

        public MultiFileDownloadTask(string[] url, string[] path, int threanum)
        {
            if (url.Length != path.Length) 
                throw new InvalidOperationException("url Should Contain Same Amount Of path");
            this.threadnum = threanum;
            _url = url;
            _fileName = path;
            Init();
        }

        internal MultiFileDownloadTask() { }

        internal void Set(string[] url, string[] path)
        {
            _url = url;
            _fileName = path;
            Init();
        }

        protected void Init()
        {
            int count = 0;

            for (int i = 0; i < _url.Length; i++) 
            {
                ProgressTask task;

                //If ProgressTasks Does Not Have So Many Tasks, Add A New Task
                if (ProgressTasks.Count < count + 1) Add(new SequenceProgressTask());

                task = new FileDownloadTask(_url[i], _fileName[i], 1);

                

                (ProgressTasks[count] as SequenceProgressTask).Add(task);
                count++;
                if (count == threadnum) count = 0;
            }
        }
    }
}
