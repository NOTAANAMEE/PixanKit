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
        protected string[] urls = [];

        /// <summary>
        /// The file paths where the downloaded files will be saved.
        /// </summary>
        protected string[] paths = [];

        /// <summary>
        /// The number of threads to use for downloading.
        /// </summary>
        protected int threadnum = 1;

        List<FileDownloadTask> files = [];

        /// <inheritdoc/>
        public long Size 
        {
            get
            {
                long ret = 0;
                foreach (var task in files)
                {
                    ret += task.Size;
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
                foreach (var task in files)
                {
                    ret += task .DownloadedBytes;
                }
                return ret;
            }
        }

        /// <inheritdoc/>
        public int TotalFiles 
        {
            get => paths.Length;
        }

        /// <inheritdoc/>
        public int DownloadedFiles 
        {
            get
            {
                int ret = 0;
                foreach (var task in files)
                {
                    ret += task.DownloadedFiles;
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
            urls = url;
            paths = path;
            Init();
        }

        internal MultiFileDownloadTask() { }

        internal void Set(string[] url, string[] path)
        {
            urls = url;
            paths = path;
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

            List<SequenceProgressTask> tasks = [];

            for (int i = 0; i < urls.Length; i++) 
            {
                FileDownloadTask? task;

                //If ProgressTasks Does Not Have So Many Tasks, Add A New Task
                if (ProgressTasks.Count < count + 1) tasks.Add(new SequenceProgressTask());
                task = new FileDownloadTask(urls[i], paths[i], 1);
                files.Add(task);
                tasks[count].Add(task);
                count++;
                if (count == threadnum) count = 0;
            }
            ProgressTasks = tasks.Cast<ProgressTask>().ToList();
        }
    }
}
