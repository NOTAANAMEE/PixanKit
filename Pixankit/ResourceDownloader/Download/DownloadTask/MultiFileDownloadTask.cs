using PixanKit.ResourceDownloader.Download.DownloadTask;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiProgressTask;
using ResourceDownloader.Download.DownloadTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Download.DownloadTask
{
    /// <summary>
    /// Represents a task for downloading multiple files concurrently using multiple threads.
    /// </summary>
    public class MultiFileDownloadTask: AsyncProgressTask, IFileDownload
    {
        /// <summary>
        /// The default number of threads to use for downloading.
        /// </summary>
        public static int ThreadNum = 64;

        /// <summary>
        /// The URLs of the files to download.
        /// </summary>
        protected string[] _url = [];

        /// <summary>
        /// The file paths where the downloaded files will be saved.
        /// </summary>
        protected string[] _fileName = [];

        /// <summary>
        /// The number of threads to use for downloading.
        /// </summary>
        protected int threadnum = 1;

        /// <inheritdoc/>
        public long Size 
        {
            get
            {
                long ret = 0;
                foreach (var thread in ProgressTasks)
                    foreach(var task in (thread as MultiProgressTask).ProgressTasks)
                    {
                        ret += (task as FileDownloadTask).Size;
                    }
                return ret;
            } 
        }

        /// <inheritdoc/>
        public long DownloadedBytes 
        {
            get
            {
                long ret = 0;
                foreach (var thread in ProgressTasks)
                    foreach (var task in (thread as MultiProgressTask).ProgressTasks)
                    {
                        ret += (task as FileDownloadTask).DownloadedBytes;
                    }
                return ret;
            }
        }

        /// <inheritdoc/>
        public int TotalFiles 
        {
            get => _fileName.Length;
        }

        /// <inheritdoc/>
        public int DownloadedFiles 
        {
            get
            {
                int ret = 0;
                foreach (var thread in ProgressTasks)
                    foreach (var task in (thread as MultiProgressTask).ProgressTasks)
                    {
                        ret += (task as FileDownloadTask).DownloadedFiles;
                    }
                return ret;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFileDownloadTask"/> class 
        /// with specified URLs and file paths, using the default number of threads.
        /// </summary>
        /// <param name="url">An array of file URLs to download.</param>
        /// <param name="path">An array of file paths where the files will be saved.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the length of <paramref name="url"/> and <paramref name="path"/> are not equal.
        /// </exception>
        public MultiFileDownloadTask(string[] url, string[] path) : this(url, path, ThreadNum)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFileDownloadTask"/> class 
        /// with specified URLs, file paths, and the number of threads to use.
        /// </summary>
        /// <param name="url">An array of file URLs to download.</param>
        /// <param name="path">An array of file paths where the files will be saved.</param>
        /// <param name="threanum">The number of threads to use for downloading.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the length of <paramref name="url"/> and <paramref name="path"/> are not equal.
        /// </exception>
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

        /// <summary>
        /// Initializes the download tasks by dividing the files across multiple threads.
        /// </summary>
        /// <remarks>
        /// Each file is assigned to a sequence task, ensuring that downloads are grouped based on the available threads.
        /// If more files are present than threads, files are distributed cyclically.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is a mismatch between the number of URLs and paths.
        /// </exception>
        protected void Init()
        {
            int count = 0;

            for (int i = 0; i < _url.Length; i++) 
            {
                ProgressTask? task = null;

                //If ProgressTasks Does Not Have So Many Tasks, Add A New Task
                if (ProgressTasks.Count < count + 1) Add(new SequenceProgressTask());
                try
                {
                    Console.WriteLine(i);
                    task = new FileDownloadTask(_url[i], _fileName[i], 1);
                }
                catch (Exception e) { Console.WriteLine(e.Message); }

                

                (ProgressTasks[count] as SequenceProgressTask).Add(task);
                count++;
                if (count == threadnum) count = 0;
            }
        }
    }
}
